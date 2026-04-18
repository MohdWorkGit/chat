using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Portals.Commands;

public record CreatePortalCommand(
    string Name = "",
    string? Slug = null,
    int AccountId = 0,
    string? CustomDomain = null,
    string? Color = null,
    string? HeaderText = null,
    string? PageTitle = null,
    string? HomepageLink = null) : IRequest<long>;

public record UpdatePortalCommand(
    long Id = 0,
    string? Name = null,
    string? Slug = null,
    string? CustomDomain = null,
    string? Color = null,
    string? HeaderText = null,
    string? PageTitle = null,
    string? HomepageLink = null,
    bool? Archived = null) : IRequest;

public record DeletePortalCommand(long Id) : IRequest;

public record UploadPortalLogoCommand(
    int PortalId,
    byte[] FileBytes,
    string FileName,
    string ContentType) : IRequest<object>;

public class CreatePortalCommandHandler : IRequestHandler<CreatePortalCommand, long>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePortalCommandHandler(IRepository<Portal> portalRepository, IUnitOfWork unitOfWork)
    {
        _portalRepository = portalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreatePortalCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Portal name is required.", nameof(request.Name));

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Name)
            : Slugify(request.Slug);

        if (await _portalRepository.AnyAsync(p => p.Slug == slug, cancellationToken))
            throw new InvalidOperationException($"A portal with slug '{slug}' already exists.");

        var portal = new Portal
        {
            AccountId = request.AccountId,
            Name = request.Name,
            Slug = slug,
            CustomDomain = request.CustomDomain,
            Color = request.Color,
            HeaderText = request.HeaderText,
            PageTitle = request.PageTitle,
            HomepageLink = request.HomepageLink,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _portalRepository.AddAsync(portal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return portal.Id;
    }

    private static string Slugify(string value) =>
        value.Trim().ToLowerInvariant().Replace(" ", "-").Replace("--", "-").Trim('-');
}

public class UpdatePortalCommandHandler : IRequestHandler<UpdatePortalCommand>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePortalCommandHandler(IRepository<Portal> portalRepository, IUnitOfWork unitOfWork)
    {
        _portalRepository = portalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePortalCommand request, CancellationToken cancellationToken)
    {
        var portal = await _portalRepository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Portal {request.Id} not found.");

        if (request.Name is not null)
            portal.Name = request.Name;
        if (request.Slug is not null)
        {
            var newSlug = request.Slug.Trim().ToLowerInvariant().Replace(" ", "-").Replace("--", "-").Trim('-');
            if (newSlug != portal.Slug &&
                await _portalRepository.AnyAsync(p => p.Slug == newSlug && p.Id != portal.Id, cancellationToken))
            {
                throw new InvalidOperationException($"A portal with slug '{newSlug}' already exists.");
            }
            portal.Slug = newSlug;
        }
        if (request.CustomDomain is not null)
            portal.CustomDomain = request.CustomDomain;
        if (request.Color is not null)
            portal.Color = request.Color;
        if (request.HeaderText is not null)
            portal.HeaderText = request.HeaderText;
        if (request.PageTitle is not null)
            portal.PageTitle = request.PageTitle;
        if (request.HomepageLink is not null)
            portal.HomepageLink = request.HomepageLink;
        if (request.Archived.HasValue)
            portal.Archived = request.Archived.Value;

        portal.UpdatedAt = DateTime.UtcNow;
        _portalRepository.Update(portal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class DeletePortalCommandHandler : IRequestHandler<DeletePortalCommand>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePortalCommandHandler(IRepository<Portal> portalRepository, IUnitOfWork unitOfWork)
    {
        _portalRepository = portalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePortalCommand request, CancellationToken cancellationToken)
    {
        var portal = await _portalRepository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Portal {request.Id} not found.");

        _portalRepository.Remove(portal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class UploadPortalLogoCommandHandler : IRequestHandler<UploadPortalLogoCommand, object>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadPortalLogoCommandHandler(
        IRepository<Portal> portalRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _portalRepository = portalRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(UploadPortalLogoCommand request, CancellationToken cancellationToken)
    {
        var portal = await _portalRepository.GetByIdAsync(request.PortalId, cancellationToken)
            ?? throw new InvalidOperationException($"Portal {request.PortalId} not found.");

        var extension = Path.GetExtension(request.FileName);
        var key = $"portals/{portal.AccountId}/{portal.Id}/logo{extension}";

        using var stream = new MemoryStream(request.FileBytes);
        await _storageService.UploadFileAsync(key, stream, request.ContentType, cancellationToken);

        portal.LogoUrl = key;
        portal.LogoContentType = request.ContentType;
        portal.UpdatedAt = DateTime.UtcNow;

        _portalRepository.Update(portal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { portal.Id, portal.LogoUrl };
    }
}
