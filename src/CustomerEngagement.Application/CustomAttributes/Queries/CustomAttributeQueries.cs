using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CustomAttributes.Queries;

public record GetCustomAttributesQuery(long AccountId, string? AppliedTo) : IRequest<object>;

public record GetCustomAttributeByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetCustomAttributesQueryHandler : IRequestHandler<GetCustomAttributesQuery, object>
{
    private readonly IRepository<CustomAttributeDefinition> _repository;

    public GetCustomAttributesQueryHandler(IRepository<CustomAttributeDefinition> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<object> Handle(GetCustomAttributesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<CustomAttributeDefinition> attributes;

        if (!string.IsNullOrWhiteSpace(request.AppliedTo))
        {
            var appliedTo = request.AppliedTo!.ToLowerInvariant();
            attributes = await _repository.FindAsync(
                ca => ca.AccountId == (int)request.AccountId && ca.AttributeModel == appliedTo,
                cancellationToken);
        }
        else
        {
            attributes = await _repository.FindAsync(
                ca => ca.AccountId == (int)request.AccountId,
                cancellationToken);
        }

        return attributes.Select(CustomAttributeMapping.ToDto).ToList();
    }
}

public class GetCustomAttributeByIdQueryHandler : IRequestHandler<GetCustomAttributeByIdQuery, object>
{
    private readonly IRepository<CustomAttributeDefinition> _repository;

    public GetCustomAttributeByIdQueryHandler(IRepository<CustomAttributeDefinition> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<object> Handle(GetCustomAttributeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.FindOneAsync(
            ca => ca.AccountId == (int)request.AccountId && ca.Id == (int)request.Id,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Custom attribute {request.Id} not found.");

        return CustomAttributeMapping.ToDto(entity);
    }
}
