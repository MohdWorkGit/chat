using MediatR;

namespace CustomerEngagement.Application.Portals.Commands;

public record CreatePortalCommand(string Name = "", string? Slug = null) : IRequest<object>;

public record UpdatePortalCommand(long Id = 0, string? Name = null, string? Slug = null) : IRequest<object>;

public record DeletePortalCommand(long Id) : IRequest<object>;
