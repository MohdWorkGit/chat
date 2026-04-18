using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;

namespace CustomerEngagement.Application.Services.HelpCenter;

public class PortalService : IPortalService
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PortalService(
        IRepository<Portal> portalRepository,
        IUnitOfWork unitOfWork)
    {
        _portalRepository = portalRepository ?? throw new ArgumentNullException(nameof(portalRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PortalDto?> GetByIdAsync(int portalId, CancellationToken cancellationToken = default)
    {
        var portal = await _portalRepository.GetByIdAsync(portalId, cancellationToken);
        return portal is null ? null : MapToDto(portal);
    }

    public async Task<PortalDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var portals = await _portalRepository.ListAsync(new { Slug = slug }, cancellationToken);
        var portal = portals.FirstOrDefault();
        return portal is null ? null : MapToDto(portal);
    }

    public async Task<IEnumerable<PortalDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var portals = await _portalRepository.ListAsync(new { AccountId = accountId }, cancellationToken);
        return portals.Select(MapToDto);
    }

    public async Task<PortalDto> CreateAsync(int accountId, CreatePortalRequest request, CancellationToken cancellationToken = default)
    {
        var portal = new Portal
        {
            AccountId = accountId,
            Name = request.Name,
            Slug = request.Slug,
            CustomDomain = request.CustomDomain,
            HeaderText = request.HeaderText,
            PageTitle = request.PageTitle,
            HomepageLink = request.HomepageLink,
            Color = request.Color,
            Archived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _portalRepository.AddAsync(portal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(portal);
    }

    public async Task<PortalDto> UpdateAsync(int portalId, UpdatePortalRequest request, CancellationToken cancellationToken = default)
    {
        var portal = await _portalRepository.GetByIdAsync(portalId, cancellationToken)
            ?? throw new InvalidOperationException($"Portal {portalId} not found.");

        if (request.Name is not null) portal.Name = request.Name;
        if (request.CustomDomain is not null) portal.CustomDomain = request.CustomDomain;
        if (request.HeaderText is not null) portal.HeaderText = request.HeaderText;
        if (request.PageTitle is not null) portal.PageTitle = request.PageTitle;
        if (request.HomepageLink is not null) portal.HomepageLink = request.HomepageLink;
        if (request.Color is not null) portal.Color = request.Color;
        if (request.IsArchived.HasValue) portal.Archived = request.IsArchived.Value;
        portal.UpdatedAt = DateTime.UtcNow;

        await _portalRepository.UpdateAsync(portal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(portal);
    }

    public async Task DeleteAsync(int portalId, CancellationToken cancellationToken = default)
    {
        var portal = await _portalRepository.GetByIdAsync(portalId, cancellationToken)
            ?? throw new InvalidOperationException($"Portal {portalId} not found.");

        await _portalRepository.DeleteAsync(portal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PortalDto MapToDto(Portal portal)
    {
        return new PortalDto
        {
            Id = portal.Id,
            AccountId = portal.AccountId,
            Name = portal.Name,
            Slug = portal.Slug,
            CustomDomain = portal.CustomDomain,
            HeaderText = portal.HeaderText,
            PageTitle = portal.PageTitle,
            HomepageLink = portal.HomepageLink,
            Color = portal.Color,
            LogoUrl = portal.LogoUrl,
            LogoContentType = portal.LogoContentType,
            IsArchived = portal.Archived,
            CreatedAt = portal.CreatedAt,
            UpdatedAt = portal.UpdatedAt
        };
    }
}
