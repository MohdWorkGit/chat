using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Application.Services.Automations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Executes ongoing campaigns that match conversation trigger conditions.
/// Runs as a recurring Hangfire job.
/// </summary>
public class ScheduledCampaignJob
{
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<ScheduledCampaignJob> _logger;

    public ScheduledCampaignJob(
        IRepository<Campaign> campaignRepository,
        IRepository<Conversation> conversationRepository,
        IMediator mediator,
        ILogger<ScheduledCampaignJob> logger)
    {
        _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ongoing campaign evaluation job");

        var ongoingCampaigns = await _campaignRepository.FindAsync(
            c => c.Enabled && c.CampaignType == CampaignType.Ongoing,
            cancellationToken);

        if (!ongoingCampaigns.Any())
        {
            _logger.LogDebug("No ongoing campaigns to process");
            return;
        }

        foreach (var campaign in ongoingCampaigns)
        {
            try
            {
                await EvaluateCampaignAsync(campaign, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to evaluate ongoing campaign {CampaignId}", campaign.Id);
            }
        }

        _logger.LogInformation("Ongoing campaign evaluation completed. Evaluated {Count} campaigns", ongoingCampaigns.Count);
    }

    private async Task EvaluateCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        // Find conversations that match campaign criteria (e.g., open conversations in the campaign's inbox)
        var matchingConversations = await _conversationRepository.FindAsync(
            c => c.AccountId == campaign.AccountId
                 && c.Status == ConversationStatus.Open
                 && c.LastActivityAt >= DateTime.UtcNow.AddMinutes(-10),
            cancellationToken);

        foreach (var conversation in matchingConversations)
        {
            await _mediator.Publish(
                new CampaignMessageEvent(
                    campaign.Id,
                    campaign.AccountId,
                    conversation.ContactId,
                    campaign.Message ?? string.Empty),
                cancellationToken);
        }
    }
}
