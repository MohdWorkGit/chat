using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CustomFilters.Queries;

public record GetCustomFiltersQuery(long AccountId, string? FilterType) : IRequest<object>;

public record GetCustomFilterByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetCustomFiltersQueryHandler : IRequestHandler<GetCustomFiltersQuery, object>
{
    private readonly IRepository<CustomFilter> _customFilterRepository;

    public GetCustomFiltersQueryHandler(IRepository<CustomFilter> customFilterRepository)
    {
        _customFilterRepository = customFilterRepository ?? throw new ArgumentNullException(nameof(customFilterRepository));
    }

    public async Task<object> Handle(GetCustomFiltersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<CustomFilter> filters;

        if (!string.IsNullOrEmpty(request.FilterType))
        {
            filters = await _customFilterRepository.FindAsync(
                cf => cf.AccountId == (int)request.AccountId && cf.FilterType == request.FilterType,
                cancellationToken);
        }
        else
        {
            filters = await _customFilterRepository.FindAsync(
                cf => cf.AccountId == (int)request.AccountId,
                cancellationToken);
        }

        return filters.Select(cf => new
        {
            cf.Id,
            cf.AccountId,
            cf.Name,
            cf.FilterType,
            cf.Query,
            cf.CreatedAt
        }).ToList();
    }
}

public class GetCustomFilterByIdQueryHandler : IRequestHandler<GetCustomFilterByIdQuery, object>
{
    private readonly IRepository<CustomFilter> _customFilterRepository;

    public GetCustomFilterByIdQueryHandler(IRepository<CustomFilter> customFilterRepository)
    {
        _customFilterRepository = customFilterRepository ?? throw new ArgumentNullException(nameof(customFilterRepository));
    }

    public async Task<object> Handle(GetCustomFilterByIdQuery request, CancellationToken cancellationToken)
    {
        var filters = await _customFilterRepository.FindAsync(
            cf => cf.AccountId == (int)request.AccountId && cf.Id == (int)request.Id,
            cancellationToken);

        var filter = filters.FirstOrDefault();

        if (filter is null)
            return new { Error = "Custom filter not found" };

        return new
        {
            filter.Id,
            filter.AccountId,
            filter.Name,
            filter.FilterType,
            filter.Query,
            filter.CreatedAt
        };
    }
}
