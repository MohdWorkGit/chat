using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class ReopenSnoozedConversationsJob
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ReopenSnoozedConversationsJob> _logger;

    public ReopenSnoozedConversationsJob(
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ReopenSnoozedConversationsJob> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Reopens snoozed conversations whose snooze period has expired.
    /// Intended to be scheduled by Hangfire as a recurring job (every 5 minutes).
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting reopen snoozed conversations job");

        var now = DateTime.UtcNow;

        var snoozedConversations = await _conversationRepository.FindAsync(
            c => c.Status == ConversationStatus.Snoozed
                 && c.SnoozedUntil != null
                 && c.SnoozedUntil <= now,
            cancellationToken);

        if (!snoozedConversations.Any())
        {
            _logger.LogDebug("No snoozed conversations to reopen");
            return;
        }

        var reopenedCount = 0;

        foreach (var conversation in snoozedConversations)
        {
            try
            {
                conversation.Status = ConversationStatus.Open;
                conversation.SnoozedUntil = null;
                conversation.UpdatedAt = DateTime.UtcNow;

                await _conversationRepository.UpdateAsync(conversation, cancellationToken);

                await _mediator.Publish(
                    new ConversationStatusChangedEvent(
                        conversation.Id,
                        conversation.AccountId,
                        nameof(ConversationStatus.Snoozed),
                        nameof(ConversationStatus.Open)),
                    cancellationToken);

                reopenedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to reopen snoozed conversation {ConversationId}",
                    conversation.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reopen snoozed conversations job completed. Reopened: {Count}", reopenedCount);
    }
}
