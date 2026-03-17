using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Reporting;

public interface ICsatReportService
{
    Task<CsatReportDto> GetCsatReportAsync(int accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<CsatReportDto> GetAgentCsatAsync(int accountId, int agentId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<CsatTrendDto> GetCsatTrendAsync(int accountId, DateTime from, DateTime to, string groupBy, CancellationToken cancellationToken = default);
}

public class CsatReportService : ICsatReportService
{
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly ILogger<CsatReportService> _logger;

    public CsatReportService(
        IRepository<CsatSurveyResponse> csatRepository,
        ILogger<CsatReportService> logger)
    {
        _csatRepository = csatRepository ?? throw new ArgumentNullException(nameof(csatRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CsatReportDto> GetCsatReportAsync(
        int accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var responses = await _csatRepository.FindAsync(
            r => r.AccountId == accountId && r.CreatedAt >= from && r.CreatedAt <= to,
            cancellationToken);

        return BuildCsatReport(responses);
    }

    public async Task<CsatReportDto> GetAgentCsatAsync(
        int accountId,
        int agentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var responses = await _csatRepository.FindAsync(
            r => r.AccountId == accountId
                && r.AssigneeId == agentId
                && r.CreatedAt >= from
                && r.CreatedAt <= to,
            cancellationToken);

        return BuildCsatReport(responses);
    }

    public async Task<CsatTrendDto> GetCsatTrendAsync(
        int accountId,
        DateTime from,
        DateTime to,
        string groupBy,
        CancellationToken cancellationToken = default)
    {
        var responses = await _csatRepository.FindAsync(
            r => r.AccountId == accountId && r.CreatedAt >= from && r.CreatedAt <= to,
            cancellationToken);

        var responseList = responses.ToList();
        var dataPoints = new List<CsatTrendDataPointDto>();

        var currentDate = from.Date;
        while (currentDate <= to.Date)
        {
            var nextDate = groupBy.ToLowerInvariant() switch
            {
                "week" => currentDate.AddDays(7),
                "month" => currentDate.AddMonths(1),
                _ => currentDate.AddDays(1)
            };

            var periodResponses = responseList
                .Where(r => r.CreatedAt >= currentDate && r.CreatedAt < nextDate)
                .ToList();

            var ratedResponses = periodResponses.Where(r => r.Rating.HasValue).ToList();

            dataPoints.Add(new CsatTrendDataPointDto
            {
                Date = currentDate,
                AverageRating = ratedResponses.Count > 0
                    ? Math.Round(ratedResponses.Average(r => r.Rating!.Value), 2)
                    : 0,
                TotalResponses = periodResponses.Count,
                Label = currentDate.ToString("yyyy-MM-dd")
            });

            currentDate = nextDate;
        }

        return new CsatTrendDto
        {
            AccountId = accountId,
            From = from,
            To = to,
            GroupBy = groupBy,
            DataPoints = dataPoints
        };
    }

    private static CsatReportDto BuildCsatReport(IReadOnlyList<CsatSurveyResponse> responses)
    {
        var ratedResponses = responses.Where(r => r.Rating.HasValue).ToList();

        var ratingDistribution = ratedResponses
            .GroupBy(r => r.Rating!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        // Satisfied: rating 4-5, Neutral: rating 3, Unsatisfied: rating 1-2
        var satisfiedCount = ratedResponses.Count(r => r.Rating!.Value >= 4);
        var neutralCount = ratedResponses.Count(r => r.Rating!.Value == 3);
        var unsatisfiedCount = ratedResponses.Count(r => r.Rating!.Value <= 2);

        return new CsatReportDto
        {
            AverageRating = ratedResponses.Count > 0
                ? Math.Round(ratedResponses.Average(r => r.Rating!.Value), 2)
                : 0,
            TotalResponses = responses.Count,
            SatisfiedCount = satisfiedCount,
            NeutralCount = neutralCount,
            UnsatisfiedCount = unsatisfiedCount,
            RatingDistribution = ratingDistribution
        };
    }
}

public class CsatReportDto
{
    public double AverageRating { get; set; }
    public int TotalResponses { get; set; }
    public int SatisfiedCount { get; set; }
    public int NeutralCount { get; set; }
    public int UnsatisfiedCount { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}

public class CsatTrendDto
{
    public int AccountId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string GroupBy { get; set; } = "day";
    public List<CsatTrendDataPointDto> DataPoints { get; set; } = [];
}

public class CsatTrendDataPointDto
{
    public DateTime Date { get; set; }
    public double AverageRating { get; set; }
    public int TotalResponses { get; set; }
    public string Label { get; set; } = string.Empty;
}
