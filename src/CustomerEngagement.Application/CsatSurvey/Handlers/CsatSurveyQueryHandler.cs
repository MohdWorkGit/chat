using CustomerEngagement.Application.CsatSurvey.Queries;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CsatSurvey.Handlers;

public class GetCsatResponsesHandler : IRequestHandler<GetCsatResponsesQuery, object>
{
    private readonly IRepository<CsatSurveyResponse> _csatRepository;

    public GetCsatResponsesHandler(IRepository<CsatSurveyResponse> csatRepository)
    {
        _csatRepository = csatRepository;
    }

    public async Task<object> Handle(GetCsatResponsesQuery request, CancellationToken cancellationToken)
    {
        var responses = await _csatRepository.FindAsync(
            r => r.AccountId == request.AccountId
                && (!request.Since.HasValue || r.CreatedAt >= request.Since.Value)
                && (!request.Until.HasValue || r.CreatedAt <= request.Until.Value),
            cancellationToken);

        var responseList = responses
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new
            {
                r.Id,
                r.ConversationId,
                r.ContactId,
                r.AssigneeId,
                r.Rating,
                r.FeedbackText,
                r.CreatedAt
            })
            .ToList();

        return new
        {
            Data = responseList,
            Total = responses.Count,
            request.Page,
            request.PageSize
        };
    }
}

public class GetCsatMetricsHandler : IRequestHandler<GetCsatMetricsQuery, object>
{
    private readonly IRepository<CsatSurveyResponse> _csatRepository;

    public GetCsatMetricsHandler(IRepository<CsatSurveyResponse> csatRepository)
    {
        _csatRepository = csatRepository;
    }

    public async Task<object> Handle(GetCsatMetricsQuery request, CancellationToken cancellationToken)
    {
        var responses = await _csatRepository.FindAsync(
            r => r.AccountId == request.AccountId
                && (!request.Since.HasValue || r.CreatedAt >= request.Since.Value)
                && (!request.Until.HasValue || r.CreatedAt <= request.Until.Value),
            cancellationToken);

        var ratedResponses = responses.Where(r => r.Rating.HasValue).ToList();

        return new
        {
            TotalResponses = responses.Count,
            AverageRating = ratedResponses.Count > 0
                ? Math.Round(ratedResponses.Average(r => r.Rating!.Value), 2)
                : 0,
            SatisfactionScore = ratedResponses.Count > 0
                ? Math.Round((double)ratedResponses.Count(r => r.Rating!.Value >= 4) / ratedResponses.Count * 100, 1)
                : 0,
            RatingDistribution = Enumerable.Range(1, 5).ToDictionary(
                rating => rating,
                rating => ratedResponses.Count(r => r.Rating!.Value == rating))
        };
    }
}
