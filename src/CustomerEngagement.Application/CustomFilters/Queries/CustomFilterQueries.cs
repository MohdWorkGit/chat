using MediatR;

namespace CustomerEngagement.Application.CustomFilters.Queries;

public record GetCustomFiltersQuery(long AccountId, string? FilterType) : IRequest<object>;

public record GetCustomFilterByIdQuery(long AccountId, long Id) : IRequest<object>;
