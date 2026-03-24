using MediatR;

namespace CustomerEngagement.Application.CustomAttributes.Commands;

public record CreateCustomAttributeCommand(long AccountId = 0, string AttributeDisplayName = "", string? AttributeKey = null) : IRequest<object>;

public record UpdateCustomAttributeCommand(long AccountId = 0, long Id = 0, string? AttributeDisplayName = null) : IRequest<object>;

public record DeleteCustomAttributeCommand(long AccountId, long Id) : IRequest<object>;
