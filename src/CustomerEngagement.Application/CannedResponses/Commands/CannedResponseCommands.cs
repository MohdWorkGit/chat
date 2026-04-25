using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CannedResponses.Commands;

public record CreateCannedResponseCommand(long AccountId = 0, string ShortCode = "", string? Content = null) : IRequest<object>;

public record UpdateCannedResponseCommand(long AccountId = 0, long Id = 0, string? ShortCode = null, string? Content = null) : IRequest<object>;

public record DeleteCannedResponseCommand(long AccountId, long Id) : IRequest<object>;

public class CreateCannedResponseCommandHandler : IRequestHandler<CreateCannedResponseCommand, object>
{
    private readonly IRepository<CannedResponse> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCannedResponseCommandHandler(IRepository<CannedResponse> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(CreateCannedResponseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ShortCode))
            throw new ArgumentException("Short code is required.", nameof(request.ShortCode));
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Content is required.", nameof(request.Content));

        var now = DateTime.UtcNow;
        var entity = new CannedResponse
        {
            AccountId = (int)request.AccountId,
            ShortCode = request.ShortCode,
            Content = request.Content!,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(entity, cancellationToken);
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
    private readonly IRepository<CannedResponse> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCannedResponseCommandHandler(IRepository<CannedResponse> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateCannedResponseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.FindOneAsync(
            cr => cr.Id == (int)request.Id && cr.AccountId == (int)request.AccountId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Canned response {request.Id} not found.");

        if (request.ShortCode is not null)
            entity.ShortCode = request.ShortCode;
        if (request.Content is not null)
            entity.Content = request.Content;

        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
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
    private readonly IRepository<CannedResponse> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCannedResponseCommandHandler(IRepository<CannedResponse> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteCannedResponseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.FindOneAsync(
            cr => cr.Id == (int)request.Id && cr.AccountId == (int)request.AccountId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Canned response {request.Id} not found.");

        _repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { entity.Id };
    }
}
