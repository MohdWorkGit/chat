using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class CsatEventHandler : INotificationHandler<ConversationStatusChangedEvent>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<CsatEventHandler> _logger;

    public CsatEventHandler(
        IRepository<Conversation> conversationRepository,
        IRepository<Inbox> inboxRepository,
        IRepository<Message> messageRepository,
        IRepository<CsatSurveyResponse> csatRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IBackgroundJobClient jobClient,
        ILogger<CsatEventHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _inboxRepository = inboxRepository;
        _messageRepository = messageRepository;
        _csatRepository = csatRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async Task Handle(ConversationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // Only trigger CSAT survey when a conversation is resolved
        if (!string.Equals(notification.NewStatus, nameof(ConversationStatus.Resolved), StringComparison.OrdinalIgnoreCase))
            return;

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
        if (conversation is null)
            return;

        var inbox = await _inboxRepository.GetByIdAsync(conversation.InboxId, cancellationToken);
        if (inbox is null || !inbox.CsatSurveyEnabled)
            return;

        _logger.LogInformation("Creating CSAT survey for resolved Conversation {ConversationId} in Inbox {InboxId}",
            notification.ConversationId, inbox.Id);

        // Create the CSAT survey message in the conversation
        var surveyMessage = new Message
        {
            ConversationId = conversation.Id,
            AccountId = conversation.AccountId,
            Content = "Please rate your conversation experience.",
            ContentType = "input_csat",
            MessageType = MessageType.Template,
            SenderType = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(surveyMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create the CSAT survey response record (awaiting customer input)
        var csatResponse = new CsatSurveyResponse
        {
            AccountId = conversation.AccountId,
            ConversationId = conversation.Id,
            MessageId = surveyMessage.Id,
            ContactId = conversation.ContactId,
            AssigneeId = conversation.AssigneeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _csatRepository.AddAsync(csatResponse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast the survey message so the widget renders it immediately
        // on resolution instead of only after the next page load.
        await _mediator.Publish(
            new MessageCreatedEvent(surveyMessage.Id, conversation.Id, conversation.AccountId),
            cancellationToken);

        // Enqueue email delivery of the survey link for the email channel.
        _jobClient.Enqueue<CsatSurveyJob>(job =>
            job.ExecuteAsync(conversation.Id, CancellationToken.None));

        _logger.LogInformation("CSAT survey created: Message {MessageId}, CsatSurveyResponse {CsatId}",
            surveyMessage.Id, csatResponse.Id);
    }
}
