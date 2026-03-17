using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class ReportingEventHandler :
    INotificationHandler<ConversationCreatedEvent>,
    INotificationHandler<ConversationStatusChangedEvent>,
    INotificationHandler<MessageCreatedEvent>,
    INotificationHandler<ConversationAssignedEvent>
{
    private readonly IRepository<ReportingEvent> _reportingEventRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportingEventHandler> _logger;

    public ReportingEventHandler(
        IRepository<ReportingEvent> reportingEventRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportingEventHandler> logger)
    {
        _reportingEventRepository = reportingEventRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording reporting event: conversation_created for Conversation {ConversationId}",
            notification.ConversationId);

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);

        var reportingEvent = new ReportingEvent
        {
            AccountId = notification.AccountId,
            Name = "conversation_created",
            ConversationId = notification.ConversationId,
            InboxId = conversation?.InboxId,
            EventStartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reportingEventRepository.AddAsync(reportingEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ConversationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (string.Equals(notification.NewStatus, nameof(ConversationStatus.Resolved), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Recording reporting event: conversation_resolved for Conversation {ConversationId}",
                notification.ConversationId);

            var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
            var now = DateTime.UtcNow;

            // Calculate resolution time in seconds
            double? resolutionTimeSeconds = null;
            if (conversation is not null)
            {
                resolutionTimeSeconds = (now - conversation.CreatedAt).TotalSeconds;
            }

            var reportingEvent = new ReportingEvent
            {
                AccountId = notification.AccountId,
                Name = "conversation_resolved",
                Value = resolutionTimeSeconds,
                ConversationId = notification.ConversationId,
                InboxId = conversation?.InboxId,
                UserId = conversation?.AssigneeId,
                EventStartedAt = conversation?.CreatedAt,
                EventEndedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _reportingEventRepository.AddAsync(reportingEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording reporting event: message_created for Message {MessageId}",
            notification.MessageId);

        var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);

        var reportingEvent = new ReportingEvent
        {
            AccountId = notification.AccountId,
            Name = "message_created",
            ConversationId = notification.ConversationId,
            InboxId = conversation?.InboxId,
            UserId = message?.SenderId,
            EventStartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reportingEventRepository.AddAsync(reportingEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Track first_response_time if this is the first outgoing message
        if (message is not null && message.MessageType == MessageType.Outgoing && conversation is not null)
        {
            var existingMessages = await _messageRepository.FindAsync(
                m => m.ConversationId == notification.ConversationId && m.MessageType == MessageType.Outgoing,
                cancellationToken);

            // Only record first_response_time if this is the first outgoing message
            if (existingMessages.Count == 1)
            {
                var firstResponseTime = (message.CreatedAt - conversation.CreatedAt).TotalSeconds;

                _logger.LogInformation("Recording reporting event: first_response_time ({ResponseTime}s) for Conversation {ConversationId}",
                    firstResponseTime, notification.ConversationId);

                var firstResponseEvent = new ReportingEvent
                {
                    AccountId = notification.AccountId,
                    Name = "first_response_time",
                    Value = firstResponseTime,
                    ConversationId = notification.ConversationId,
                    InboxId = conversation.InboxId,
                    UserId = message.SenderId,
                    EventStartedAt = conversation.CreatedAt,
                    EventEndedAt = message.CreatedAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _reportingEventRepository.AddAsync(firstResponseEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task Handle(ConversationAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording reporting event: agent_assigned for Conversation {ConversationId}",
            notification.ConversationId);

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);

        var reportingEvent = new ReportingEvent
        {
            AccountId = notification.AccountId,
            Name = "agent_assigned",
            ConversationId = notification.ConversationId,
            InboxId = conversation?.InboxId,
            UserId = notification.AssigneeId,
            EventStartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reportingEventRepository.AddAsync(reportingEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
