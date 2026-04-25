using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CannedResponses.Commands;

public record CreateCannedResponseCommand(long AccountId = 0, string ShortCode = "", string? Content = null) : IRequest<object>;

public record UpdateCannedResponseCommand(long AccountId = 0, long Id = 0, string? ShortCode = null, string? Content = null) : IRequest<object>;

public record DeleteCannedResponseCommand(long AccountId, long Id) : IRequest<object>;

public class CreateCannedResponseCommandHandler : IRequestHandler<CreateCannedResponseCommand, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCannedResponseCommandHandler(
        IRepository<CannedResponse> cannedResponseRepository,
        IUnitOfWork unitOfWork)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(CreateCannedResponseCommand request, CancellationToken cancellationToken)
    {
        var shortCode = (request.ShortCode ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(shortCode))
            return new { Error = "Short code is required." };
        if (string.IsNullOrWhiteSpace(request.Content))
            return new { Error = "Content is required." };

        var accountId = (int)request.AccountId;
        var duplicate = await _cannedResponseRepository.AnyAsync(
            cr => cr.AccountId == accountId && cr.ShortCode == shortCode,
            cancellationToken);

        if (duplicate)
            return new { Error = "A canned response with this short code already exists." };

        var now = DateTime.UtcNow;
        var entity = new CannedResponse
        {
            AccountId = accountId,
            ShortCode = shortCode,
            Content = request.Content!,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _cannedResponseRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            entity.Id,
            entity.AccountId,
            entity.ShortCode,
            entity.Content,
            entity.CreatedAt,
            entity.UpdatedAt
        };
    }
}

public class UpdateCannedResponseCommandHandler : IRequestHandler<UpdateCannedResponseCommand, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCannedResponseCommandHandler(
        IRepository<CannedResponse> cannedResponseRepository,
        IUnitOfWork unitOfWork)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateCannedResponseCommand request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var id = (int)request.Id;

        var existing = await _cannedResponseRepository.FindAsync(
            cr => cr.AccountId == accountId && cr.Id == id,
            cancellationToken);

        var entity = existing.FirstOrDefault();
        if (entity is null)
            return new { Error = "Canned response not found" };

        if (request.ShortCode is not null)
        {
            var shortCode = request.ShortCode.Trim();
            if (string.IsNullOrEmpty(shortCode))
                return new { Error = "Short code cannot be empty." };

            if (!string.Equals(shortCode, entity.ShortCode, StringComparison.Ordinal))
            {
                var duplicate = await _cannedResponseRepository.AnyAsync(
                    cr => cr.AccountId == accountId && cr.Id != id && cr.ShortCode == shortCode,
                    cancellationToken);

                if (duplicate)
                    return new { Error = "A canned response with this short code already exists." };
            }

            entity.ShortCode = shortCode;
        }

        if (request.Content is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new { Error = "Content cannot be empty." };
            entity.Content = request.Content;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        _cannedResponseRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            entity.Id,
            entity.AccountId,
            entity.ShortCode,
            entity.Content,
            entity.CreatedAt,
            entity.UpdatedAt
        };
    }
}

public class DeleteCannedResponseCommandHandler : IRequestHandler<DeleteCannedResponseCommand, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCannedResponseCommandHandler(
        IRepository<CannedResponse> cannedResponseRepository,
        IUnitOfWork unitOfWork)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteCannedResponseCommand request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var id = (int)request.Id;

        var existing = await _cannedResponseRepository.FindAsync(
            cr => cr.AccountId == accountId && cr.Id == id,
            cancellationToken);

        var entity = existing.FirstOrDefault();
        if (entity is null)
            return new { Error = "Canned response not found" };

        _cannedResponseRepository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { Success = true };
    }
}
