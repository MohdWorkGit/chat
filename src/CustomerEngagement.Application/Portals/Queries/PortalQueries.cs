using MediatR;

namespace CustomerEngagement.Application.Portals.Queries;

public record GetPortalsQuery(int Page, int PageSize) : IRequest<object>;

public record GetPortalByIdQuery(long Id) : IRequest<object>;
