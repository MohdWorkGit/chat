using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class NotificationEventHandler :
    INotificationHandler<ConversationAssignedEvent>,
    INotificationHandler<MessageCreatedEvent>,
    INotificationHandler<MentionCreatedEvent>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<ConversationParticipant> _participantRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<NotificationEventHandler> _logger;

    public NotificationEventHandler(
        IRepository<Notification> notificationRepository,
        IRepository<ConversationParticipant> participantRepository,
        IRepository<Message> messageRepository,
        IUnitOfWork unitOfWork,
        IBackgroundJobClient jobClient,
        ILogger<NotificationEventHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _participantRepository = participantRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async Task Handle(ConversationAssignedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.AssigneeId is null)
            return;

        _logger.LogInformation("Creating assignment notification for user {AssigneeId} on Conversation {ConversationId}",
            notification.AssigneeId, notification.ConversationId);

        var entity = new Notification
        {
            AccountId = notification.AccountId,
            UserId = notification.AssigneeId.Value,
            NotificationType = "conversation_assignment",
            PrimaryActorType = "Conversation",
            PrimaryActorId = notification.ConversationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _jobClient.Enqueue<NotificationDeliveryJob>(job =>
            job.ExecuteAsync(entity.Id, CancellationToken.None));
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating message notifications for participants of Conversation {ConversationId}",
            notification.ConversationId);

        var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
        if (message is null)
            return;

        var participants = await _participantRepository.FindAsync(
            p => p.ConversationId == notification.ConversationId,
            cancellationToken);

        var created = new List<Notification>();
        foreach (var participant in participants)
        {
            // Don't notify the sender about their own message
            if (participant.UserId == message.SenderId)
                continue;

            var entity = new Notification
            {
                AccountId = notification.AccountId,
                UserId = participant.UserId,
                NotificationType = "conversation_message",
                PrimaryActorType = "Conversation",
                PrimaryActorId = notification.ConversationId,
                SecondaryActorType = "Message",
                SecondaryActorId = notification.MessageId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(entity, cancellationToken);
            created.Add(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entity in created)
        {
            _jobClient.Enqueue<NotificationDeliveryJob>(job =>
                job.ExecuteAsync(entity.Id, CancellationToken.None));
        }
    }

    public async Task Handle(MentionCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating mention notification for user {MentionedUserId} in Conversation {ConversationId}",
            notification.MentionedUserId, notification.ConversationId);

        var entity = new Notification
        {
            AccountId = notification.AccountId,
            UserId = notification.MentionedUserId,
            NotificationType = "conversation_mention",
            PrimaryActorType = "Conversation",
            PrimaryActorId = notification.ConversationId,
            SecondaryActorType = "Message",
            SecondaryActorId = notification.MessageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _jobClient.Enqueue<NotificationDeliveryJob>(job =>
            job.ExecuteAsync(entity.Id, CancellationToken.None));
    }
}
