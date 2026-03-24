using MediatR;

namespace CustomerEngagement.Application.CsatSurvey.Queries;

public record GetCsatResponsesQuery(long AccountId, DateTime? Since, DateTime? Until, int Page, int PageSize) : IRequest<object>;

public record GetCsatMetricsQuery(long AccountId, DateTime? Since, DateTime? Until) : IRequest<object>;
