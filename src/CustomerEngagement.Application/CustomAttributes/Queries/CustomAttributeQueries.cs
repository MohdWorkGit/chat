using MediatR;

namespace CustomerEngagement.Application.CustomAttributes.Queries;

public record GetCustomAttributesQuery(long AccountId, string? AttributeModel) : IRequest<object>;

public record GetCustomAttributeByIdQuery(long AccountId, long Id) : IRequest<object>;
