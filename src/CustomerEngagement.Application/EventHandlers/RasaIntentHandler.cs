using System.Text.Json;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Integrations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

/// <summary>
/// Processes incoming messages through Rasa NLU when the inbox is configured
/// with a Rasa-typed AgentBot. Distinct from the generic webhook-based
/// <see cref="BotEventHandler"/>: this handler uses the Rasa REST webhook
/// contract (sender/message/metadata), persists the detected intent to the
/// conversation's AdditionalAttributes JSON, handles handoff, and delivers
/// any bot-authored reply messages back to the conversation.
/// </summary>
public sealed class RasaIntentHandler : INotificationHandler<MessageCreatedEvent>
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<AgentBotInbox> _agentBotInboxRepository;
    private readonly IRepository<AgentBot> _agentBotRepository;
    private readonly IRasaNluService _rasaNluService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<RasaIntentHandler> _logger;

    public RasaIntentHandler(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<AgentBotInbox> agentBotInboxRepository,
        IRepository<AgentBot> agentBotRepository,
        IRasaNluService rasaNluService,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<RasaIntentHandler> logger)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _agentBotInboxRepository = agentBotInboxRepository;
        _agentBotRepository = agentBotRepository;
        _rasaNluService = rasaNluService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
        if (message is null || message.MessageType != MessageType.Incoming)
            return;

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
        if (conversation is null)
            return;

        var agentBot = await ResolveRasaBotAsync(conversation.InboxId, cancellationToken);
        if (agentBot is null)
            return;

        _logger.LogInformation("Dispatching message {MessageId} to Rasa NLU for conversation {ConversationId}",
            notification.MessageId, conversation.Id);

        BotResponse response;
        try
        {
            response = await _rasaNluService.ProcessMessageAsync(
                new BotMessageRequest
                {
                    AccountId = conversation.AccountId,
                    ConversationId = conversation.Id,
                    Message = message.Content ?? string.Empty,
                    SenderIdentifier = $"contact_{conversation.ContactId}"
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rasa NLU dispatch failed for message {MessageId}", notification.MessageId);
            return;
        }

        if (!response.Success)
            return;

        await PersistDetectedIntentAsync(conversation, response, cancellationToken);

        foreach (var reply in response.Messages.Where(m => !string.IsNullOrWhiteSpace(m.Text)))
        {
            var botMessage = new Message
            {
                ConversationId = conversation.Id,
                AccountId = conversation.AccountId,
                Content = reply.Text,
                ContentType = reply.ContentType ?? "text",
                MessageType = MessageType.Outgoing,
                SenderType = "AgentBot",
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(botMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(
                new MessageCreatedEvent(botMessage.Id, conversation.Id, conversation.AccountId),
                cancellationToken);
        }

        if (response.HandoffToAgent && conversation.Status != ConversationStatus.Pending)
        {
            conversation.Status = ConversationStatus.Pending;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AgentBot?> ResolveRasaBotAsync(int inboxId, CancellationToken cancellationToken)
    {
        var inboxBots = await _agentBotInboxRepository.FindAsync(
            abi => abi.InboxId == inboxId && abi.Status == 1,
            cancellationToken);

        foreach (var link in inboxBots)
        {
            var bot = await _agentBotRepository.GetByIdAsync(link.AgentBotId, cancellationToken);
            if (bot is not null
                && !string.IsNullOrEmpty(bot.BotType)
                && string.Equals(bot.BotType, "rasa", StringComparison.OrdinalIgnoreCase))
            {
                return bot;
            }
        }

        return null;
    }

    private async Task PersistDetectedIntentAsync(
        Conversation conversation,
        BotResponse response,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(response.Intent))
            return;

        var attributes = ParseAdditionalAttributes(conversation.AdditionalAttributes);
        attributes["intent"] = response.Intent!;
        if (response.Confidence.HasValue)
            attributes["intent_confidence"] = response.Confidence.Value;

        conversation.AdditionalAttributes = JsonSerializer.Serialize(attributes);
        conversation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, object> ParseAdditionalAttributes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(raw)
                ?? new Dictionary<string, object>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }
    }
}
