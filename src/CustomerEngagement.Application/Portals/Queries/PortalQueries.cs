using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Portals.Queries;

public record GetPortalsQuery(int Page, int PageSize) : IRequest<object>;

public record GetPortalByIdQuery(long Id) : IRequest<object>;

public class GetPortalsQueryHandler : IRequestHandler<GetPortalsQuery, object>
{
    private readonly IRepository<Portal> _repository;

    public GetPortalsQueryHandler(IRepository<Portal> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPortalsQuery request, CancellationToken cancellationToken)
    {
        var portals = await _repository.GetPagedAsync(
            request.Page, request.PageSize, null, p => p.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(null, cancellationToken);

        return new
        {
            Data = portals.Select(p => new
            {
                p.Id,
                p.AccountId,
                p.Name,
                p.Slug,
                p.CustomDomain,
                p.Color,
                p.HeaderText,
                p.PageTitle,
                p.HomepageLink,
                p.Archived,
                p.CreatedAt,
                p.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetPortalByIdQueryHandler : IRequestHandler<GetPortalByIdQuery, object>
{
    private readonly IRepository<Portal> _repository;

    public GetPortalByIdQueryHandler(IRepository<Portal> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPortalByIdQuery request, CancellationToken cancellationToken)
    {
        var portal = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (portal is null)
            return null!;

        return new
        {
            portal.Id,
            portal.AccountId,
            portal.Name,
            portal.Slug,
            portal.CustomDomain,
            portal.Color,
            portal.HeaderText,
            portal.PageTitle,
            portal.HomepageLink,
            portal.Archived,
            portal.CreatedAt,
            portal.UpdatedAt
        };
    }
}
