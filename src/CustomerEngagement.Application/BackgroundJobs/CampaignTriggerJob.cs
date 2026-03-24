using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class CampaignTriggerJob
{
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CampaignTriggerJob> _logger;

    public CampaignTriggerJob(
        IRepository<Campaign> campaignRepository,
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CampaignTriggerJob> logger)
    {
        _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Triggers scheduled one-off campaigns that are due for execution.
    /// Intended to be scheduled by Hangfire as a recurring job (every minute).
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting campaign trigger job");

        var now = DateTime.UtcNow;

        // Find enabled one-off campaigns that are scheduled and due
        var dueCampaigns = await _campaignRepository.FindAsync(
            c => c.Enabled
                 && c.CampaignType == CampaignType.OneOff
                 && c.ScheduledAt != null
                 && c.ScheduledAt <= now,
            cancellationToken);

        if (!dueCampaigns.Any())
        {
            _logger.LogDebug("No campaigns due for execution");
            return;
        }

        foreach (var campaign in dueCampaigns)
        {
            try
            {
                await ExecuteCampaignAsync(campaign, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute campaign {CampaignId} ({Title})",
                    campaign.Id, campaign.Title);
            }
        }

        _logger.LogInformation("Campaign trigger job completed. Processed {Count} campaigns", dueCampaigns.Count);
    }

    private async Task ExecuteCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing campaign {CampaignId} ({Title})", campaign.Id, campaign.Title);

        // Get target contacts for this campaign's account
        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == campaign.AccountId,
            cancellationToken);

        var sentCount = 0;

        foreach (var contact in contacts)
        {
            try
            {
                await _mediator.Publish(
                    new CampaignMessageEvent(campaign.Id, campaign.AccountId, contact.Id, campaign.Message ?? string.Empty),
                    cancellationToken);

                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send campaign {CampaignId} message to contact {ContactId}",
                    campaign.Id, contact.Id);
            }
        }

        // Mark campaign as completed (disable it so it won't run again)
        campaign.Enabled = false;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Campaign {CampaignId} completed. Sent to {SentCount}/{TotalCount} contacts",
            campaign.Id, sentCount, contacts.Count);
    }
}