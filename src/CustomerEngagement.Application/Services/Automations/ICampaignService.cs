using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Automations;

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(int campaignId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CampaignDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    Task<CampaignDto> CreateAsync(int accountId, CreateCampaignRequest request, CancellationToken cancellationToken = default);

    Task<CampaignDto> UpdateAsync(int campaignId, UpdateCampaignRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int campaignId, CancellationToken cancellationToken = default);

    Task ActivateAsync(int campaignId, CancellationToken cancellationToken = default);

    Task DeactivateAsync(int campaignId, CancellationToken cancellationToken = default);

    Task ExecuteAsync(int campaignId, CancellationToken cancellationToken = default);
}

public class CreateCampaignRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CampaignType { get; set; }
    public int? InboxId { get; set; }
    public string? Audience { get; set; }
    public string? ScheduledAt { get; set; }
}

public class UpdateCampaignRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Message { get; set; }
    public string? Audience { get; set; }
    public string? ScheduledAt { get; set; }
}
