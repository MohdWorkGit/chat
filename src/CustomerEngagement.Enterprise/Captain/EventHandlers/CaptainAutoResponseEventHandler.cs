using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Enterprise.Captain.DTOs;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.EventHandlers;

/// <summary>
/// Listens for incoming customer messages and, when the conversation's inbox
/// is connected to a Captain AI assistant, generates an automatic AI reply.
/// </summary>
public sealed class CaptainAutoResponseEventHandler : INotificationHandler<MessageCreatedEvent>
{
    private const int PreviousMessageWindow = 10;

    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<CaptainInbox> _captainInboxRepository;
    private readonly IAssistantChatService _assistantChatService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CaptainAutoResponseEventHandler> _logger;

    public CaptainAutoResponseEventHandler(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Contact> contactRepository,
        IRepository<CaptainInbox> captainInboxRepository,
        IAssistantChatService assistantChatService,
        IUnitOfWork unitOfWork,
        ILogger<CaptainAutoResponseEventHandler> logger)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _contactRepository = contactRepository;
        _captainInboxRepository = captainInboxRepository;
        _assistantChatService = assistantChatService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Captain is an optional enterprise feature. A failure here (e.g. tables
        // missing because the module isn't provisioned, AI service unavailable,
        // transient DB error) must never break the caller's message-send flow,
        // so wrap the entire handler in a catch-all and log.
        try
        {
            var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
            if (message is null)
                return;

            // Only auto-respond to incoming customer messages
            if (message.MessageType != MessageType.Incoming || message.Private)
                return;

            if (string.IsNullOrWhiteSpace(message.Content))
                return;

            var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
            if (conversation is null)
                return;

            // Look up an active Captain assistant connected to this inbox
            var captainInboxes = await _captainInboxRepository.FindAsync(
                ci => ci.InboxId == conversation.InboxId && ci.Active,
                cancellationToken);

            var captainInbox = captainInboxes.FirstOrDefault();
            if (captainInbox is null)
                return;

            _logger.LogInformation(
                "Captain assistant {AssistantId} auto-responding to message {MessageId} on conversation {ConversationId}",
                captainInbox.AssistantId, message.Id, conversation.Id);

            // Build the prior conversation context (most recent N messages, oldest-first)
            var history = await _messageRepository.FindAsync(
                m => m.ConversationId == conversation.Id
                    && m.Id != message.Id
                    && !m.Private,
                cancellationToken);

            var previousMessages = history
                .OrderByDescending(m => m.Id)
                .Take(PreviousMessageWindow)
                .OrderBy(m => m.Id)
                .Select(m => new ChatMessage(
                    Role: m.MessageType == MessageType.Outgoing ? "assistant" : "user",
                    Content: m.Content ?? string.Empty))
                .ToList();

            string? contactName = null;
            string? contactEmail = null;
            if (conversation.ContactId > 0)
            {
                var contact = await _contactRepository.GetByIdAsync(conversation.ContactId, cancellationToken);
                contactName = contact?.Name;
                contactEmail = contact?.Email;
            }

            var context = new ConversationContext(
                conversation.Id,
                previousMessages,
                contactName,
                contactEmail);

            var response = await _assistantChatService.ChatAsync(
                captainInbox.AssistantId,
                message.Content!,
                context,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(response.Content))
                return;

            var replyMessage = new Message
            {
                ConversationId = conversation.Id,
                AccountId = conversation.AccountId,
                Content = response.Content,
                ContentType = "text",
                MessageType = MessageType.Outgoing,
                SenderType = "CaptainAssistant",
                SenderId = null,
                Status = MessageStatus.Sent,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _messageRepository.AddAsync(replyMessage, cancellationToken);

            // If the assistant requested a handoff, leave the conversation open for an
            // agent to pick up. Otherwise the message simply joins the thread.
            if (response.HandoffRequested)
            {
                _logger.LogInformation(
                    "Captain assistant {AssistantId} requested handoff on conversation {ConversationId}",
                    captainInbox.AssistantId, conversation.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Captain assistant {AssistantId} replied to message {MessageId} with new message {ReplyMessageId}",
                captainInbox.AssistantId, message.Id, replyMessage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Captain auto-response failed for message {MessageId} on conversation {ConversationId}",
                notification.MessageId, notification.ConversationId);
        }
    }
}
