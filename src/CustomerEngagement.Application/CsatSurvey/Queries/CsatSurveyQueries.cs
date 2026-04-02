using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CsatSurvey.Queries;

public record GetCsatResponsesQuery(long AccountId, DateTime? Since, DateTime? Until, int Page, int PageSize) : IRequest<object>;

public record GetCsatMetricsQuery(long AccountId, DateTime? Since, DateTime? Until) : IRequest<object>;

public class GetCsatResponsesQueryHandler : IRequestHandler<GetCsatResponsesQuery, object>
{
    private readonly IRepository<CsatSurveyResponse> _repository;

    public GetCsatResponsesQueryHandler(IRepository<CsatSurveyResponse> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetCsatResponsesQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var since = request.Since;
        var until = request.Until;

        Expression<Func<CsatSurveyResponse, bool>> predicate = r =>
            r.AccountId == accountId
            && (!since.HasValue || r.CreatedAt >= since.Value)
            && (!until.HasValue || r.CreatedAt <= until.Value);

        var responses = await _repository.GetPagedAsync(
            request.Page, request.PageSize, predicate, r => r.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(predicate, cancellationToken);

        return new
        {
            Data = responses.Select(r => new
            {
                r.Id,
                r.AccountId,
                r.ConversationId,
                r.ContactId,
                r.AssigneeId,
                r.Rating,
                r.FeedbackText,
                r.CreatedAt,
                r.UpdatedAt
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

public class GetCsatMetricsQueryHandler : IRequestHandler<GetCsatMetricsQuery, object>
{
    private readonly IRepository<CsatSurveyResponse> _repository;

    public GetCsatMetricsQueryHandler(IRepository<CsatSurveyResponse> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetCsatMetricsQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var since = request.Since;
        var until = request.Until;

        Expression<Func<CsatSurveyResponse, bool>> predicate = r =>
            r.AccountId == accountId
            && (!since.HasValue || r.CreatedAt >= since.Value)
            && (!until.HasValue || r.CreatedAt <= until.Value);

        var allResponses = await _repository.FindAsync(predicate, cancellationToken);

        var totalResponses = allResponses.Count;
        var ratedResponses = allResponses.Where(r => r.Rating.HasValue).ToList();
        var averageRating = ratedResponses.Count > 0
            ? ratedResponses.Average(r => r.Rating!.Value)
            : 0.0;
        var responseRate = totalResponses > 0
            ? (double)ratedResponses.Count / totalResponses * 100.0
            : 0.0;

        return new
        {
            TotalResponses = totalResponses,
            AverageRating = Math.Round(averageRating, 2),
            ResponseRate = Math.Round(responseRate, 2),
            RatedCount = ratedResponses.Count
        };
    }
}
