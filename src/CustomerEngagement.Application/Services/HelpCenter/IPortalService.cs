using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.HelpCenter;

public interface IPortalService
{
    Task<PortalDto?> GetByIdAsync(int portalId, CancellationToken cancellationToken = default);

    Task<PortalDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IEnumerable<PortalDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    Task<PortalDto> CreateAsync(int accountId, CreatePortalRequest request, CancellationToken cancellationToken = default);

    Task<PortalDto> UpdateAsync(int portalId, UpdatePortalRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int portalId, CancellationToken cancellationToken = default);
}
