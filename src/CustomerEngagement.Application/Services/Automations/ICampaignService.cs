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
