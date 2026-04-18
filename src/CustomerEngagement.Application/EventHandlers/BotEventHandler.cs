using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class BotEventHandler : INotificationHandler<MessageCreatedEvent>
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<AgentBotInbox> _agentBotInboxRepository;
    private readonly IRepository<AgentBot> _agentBotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BotEventHandler> _logger;

    public BotEventHandler(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<AgentBotInbox> agentBotInboxRepository,
        IRepository<AgentBot> agentBotRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IHttpClientFactory httpClientFactory,
        ILogger<BotEventHandler> logger)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _agentBotInboxRepository = agentBotInboxRepository;
        _agentBotRepository = agentBotRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
        if (message is null)
            return;

        // Only process incoming messages
        if (message.MessageType != MessageType.Incoming)
            return;

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
        if (conversation is null)
            return;

        // Check if the inbox has an associated agent bot
        var agentBotInboxes = await _agentBotInboxRepository.FindAsync(
            abi => abi.InboxId == conversation.InboxId && abi.Status == 1,
            cancellationToken);

        var agentBotInbox = agentBotInboxes.FirstOrDefault();
        if (agentBotInbox is null)
            return;

        var agentBot = await _agentBotRepository.GetByIdAsync(agentBotInbox.AgentBotId, cancellationToken);
        if (agentBot is null || string.IsNullOrWhiteSpace(agentBot.OutgoingUrl))
            return;

        // Rasa bots are routed through RasaIntentHandler, which uses the
        // Rasa REST webhook contract. Skip here so we don't double-post.
        if (!string.IsNullOrEmpty(agentBot.BotType)
            && string.Equals(agentBot.BotType, "rasa", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _logger.LogInformation("Forwarding message {MessageId} to bot {BotName} at {OutgoingUrl}",
            notification.MessageId, agentBot.Name, agentBot.OutgoingUrl);

        try
        {
            var httpClient = _httpClientFactory.CreateClient("AgentBot");

            var botPayload = new
            {
                Event = "message_created",
                MessageId = message.Id,
                ConversationId = conversation.Id,
                AccountId = conversation.AccountId,
                Content = message.Content,
                ContentType = message.ContentType,
                Sender = new
                {
                    Id = message.SenderId,
                    Type = message.SenderType
                },
                Conversation = new
                {
                    Id = conversation.Id,
                    InboxId = conversation.InboxId,
                    ContactId = conversation.ContactId,
                    Status = conversation.Status
                }
            };

            var response = await httpClient.PostAsJsonAsync(agentBot.OutgoingUrl, botPayload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bot {BotName} returned status {StatusCode} for message {MessageId}",
                    agentBot.Name, response.StatusCode, notification.MessageId);
                return;
            }

            var botResponse = await response.Content.ReadFromJsonAsync<BotResponsePayload>(cancellationToken: cancellationToken);
            if (botResponse is null || string.IsNullOrWhiteSpace(botResponse.Content))
                return;

            // Create a reply message from the bot
            var replyMessage = new Message
            {
                ConversationId = conversation.Id,
                AccountId = conversation.AccountId,
                Content = botResponse.Content,
                ContentType = botResponse.ContentType ?? "text",
                MessageType = MessageType.Outgoing,
                SenderType = "AgentBot",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(replyMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish MessageCreatedEvent so BroadcastEventHandler pushes the
            // bot reply to the widget/dashboard in real-time. Without this the
            // reply is persisted silently and the conversation view never
            // updates until a manual refresh. The Incoming-only filter at the
            // top of this handler prevents infinite recursion on the
            // Outgoing reply we just created.
            await _mediator.Publish(
                new MessageCreatedEvent(replyMessage.Id, conversation.Id, conversation.AccountId),
                cancellationToken);

            _logger.LogInformation("Bot {BotName} replied to message {MessageId} with message {ReplyMessageId}",
                agentBot.Name, notification.MessageId, replyMessage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding message {MessageId} to bot {BotName}",
                notification.MessageId, agentBot.Name);
        }
    }

    private sealed class BotResponsePayload
    {
        public string? Content { get; set; }
        public string? ContentType { get; set; }
    }
}
