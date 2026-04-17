using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Portals.Commands;

public record CreatePortalCommand(string Name = "", string? Slug = null) : IRequest<object>;

public record UpdatePortalCommand(long Id = 0, string? Name = null, string? Slug = null) : IRequest<object>;

public record DeletePortalCommand(long Id) : IRequest<object>;

public record UploadPortalLogoCommand(
    int PortalId,
    byte[] FileBytes,
    string FileName,
    string ContentType) : IRequest<object>;

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
