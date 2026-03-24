using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Automations;

public class CampaignService : ICampaignService
{
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(
        IRepository<Campaign> campaignRepository,
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CampaignService> logger)
    {
        _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CampaignDto?> GetByIdAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        return campaign is null ? null : MapToDto(campaign);
    }

    public async Task<IEnumerable<CampaignDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var campaigns = await _campaignRepository.ListAsync(new { AccountId = accountId }, cancellationToken);
        return campaigns.Select(MapToDto);
    }

    public async Task<CampaignDto> CreateAsync(int accountId, CreateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = new Campaign
        {
            AccountId = accountId,
            Title = request.Title,
            Description = request.Description,
            Message = request.Message,
            CampaignType = (CustomerEngagement.Core.Enums.CampaignType)request.CampaignType,
            InboxId = request.InboxId ?? 0,
            Audience = request.Audience,
            ScheduledAt = request.ScheduledAt is not null ? DateTime.Parse(request.ScheduledAt, null, System.Globalization.DateTimeStyles.RoundtripKind) : null,
            Enabled = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(campaign);
    }

    public async Task<CampaignDto> UpdateAsync(int campaignId, UpdateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        if (request.Title is not null) campaign.Title = request.Title;
        if (request.Description is not null) campaign.Description = request.Description;
        if (request.Message is not null) campaign.Message = request.Message;
        if (request.Audience is not null) campaign.Audience = request.Audience;
        if (request.ScheduledAt is not null) campaign.ScheduledAt = DateTime.Parse(request.ScheduledAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(campaign);
    }

    public async Task DeleteAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        await _campaignRepository.DeleteAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        campaign.Enabled = true;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        campaign.Enabled = false;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        if (!campaign.Enabled)
        {
            _logger.LogWarning("Attempted to execute disabled campaign {CampaignId}", campaignId);
            return;
        }

        // Get target contacts based on audience criteria
        var contacts = await _contactRepository.ListAsync(
            new { AccountId = campaign.AccountId },
            cancellationToken);

        foreach (var contact in contacts)
        {
            try
            {
                await _mediator.Publish(
                    new CampaignMessageEvent(campaignId, campaign.AccountId, contact.Id, campaign.Message),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send campaign message to contact {ContactId}", contact.Id);
            }
        }

        _logger.LogInformation("Campaign {CampaignId} executed for account {AccountId}", campaignId, campaign.AccountId);
    }

    private static CampaignDto MapToDto(Campaign campaign)
    {
        return new CampaignDto(
            campaign.Id,
            campaign.AccountId,
            campaign.Title,
            campaign.Description,
            campaign.Message ?? string.Empty,
            (int)campaign.CampaignType,
            campaign.InboxId,
            campaign.Enabled,
            campaign.Audience,
            campaign.ScheduledAt?.ToString("o"),
            campaign.CreatedAt,
            campaign.UpdatedAt);
    }
}

public record CampaignMessageEvent(int CampaignId, int AccountId, int ContactId, string Message) : INotification;
