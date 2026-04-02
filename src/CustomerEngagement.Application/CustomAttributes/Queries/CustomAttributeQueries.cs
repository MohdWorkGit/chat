using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CustomAttributes.Queries;

public record GetCustomAttributesQuery(long AccountId, string? AttributeModel) : IRequest<object>;

public record GetCustomAttributeByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetCustomAttributesQueryHandler : IRequestHandler<GetCustomAttributesQuery, object>
{
    private readonly IRepository<CustomAttributeDefinition> _customAttributeRepository;

    public GetCustomAttributesQueryHandler(IRepository<CustomAttributeDefinition> customAttributeRepository)
    {
        _customAttributeRepository = customAttributeRepository ?? throw new ArgumentNullException(nameof(customAttributeRepository));
    }

    public async Task<object> Handle(GetCustomAttributesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<CustomAttributeDefinition> attributes;

        if (!string.IsNullOrEmpty(request.AttributeModel))
        {
            attributes = await _customAttributeRepository.FindAsync(
                ca => ca.AccountId == (int)request.AccountId && ca.AttributeModel == request.AttributeModel,
                cancellationToken);
        }
        else
        {
            attributes = await _customAttributeRepository.FindAsync(
                ca => ca.AccountId == (int)request.AccountId,
                cancellationToken);
        }

        return attributes.Select(ca => new
        {
            ca.Id,
            ca.AccountId,
            ca.AttributeDisplayName,
            ca.AttributeDisplayType,
            ca.AttributeKey,
            ca.AttributeModel,
            ca.DefaultValue,
            ca.CreatedAt
        }).ToList();
    }
}

public class GetCustomAttributeByIdQueryHandler : IRequestHandler<GetCustomAttributeByIdQuery, object>
{
    private readonly IRepository<CustomAttributeDefinition> _customAttributeRepository;

    public GetCustomAttributeByIdQueryHandler(IRepository<CustomAttributeDefinition> customAttributeRepository)
    {
        _customAttributeRepository = customAttributeRepository ?? throw new ArgumentNullException(nameof(customAttributeRepository));
    }

    public async Task<object> Handle(GetCustomAttributeByIdQuery request, CancellationToken cancellationToken)
    {
        var attributes = await _customAttributeRepository.FindAsync(
            ca => ca.AccountId == (int)request.AccountId && ca.Id == (int)request.Id,
            cancellationToken);

        var attribute = attributes.FirstOrDefault();

        if (attribute is null)
            return new { Error = "Custom attribute not found" };

        return new
        {
            attribute.Id,
            attribute.AccountId,
            attribute.AttributeDisplayName,
            attribute.AttributeDisplayType,
            attribute.AttributeKey,
            attribute.AttributeModel,
            attribute.DefaultValue,
            attribute.CreatedAt
        };
    }
}
