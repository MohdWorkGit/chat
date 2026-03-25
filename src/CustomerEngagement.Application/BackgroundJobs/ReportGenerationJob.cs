using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Generates periodic analytics reports and stores aggregated metrics.
/// Runs as a daily recurring Hangfire job.
/// </summary>
public class ReportGenerationJob
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IRepository<ReportingEvent> _reportingEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<CsatSurveyResponse> csatRepository,
        IRepository<ReportingEvent> reportingEventRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportGenerationJob> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _csatRepository = csatRepository ?? throw new ArgumentNullException(nameof(csatRepository));
        _reportingEventRepository = reportingEventRepository ?? throw new ArgumentNullException(nameof(reportingEventRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting daily report generation job");

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var today = DateTime.UtcNow.Date;

        try
        {
            // Count conversations created yesterday
            var newConversations = await _conversationRepository.FindAsync(
                c => c.CreatedAt >= yesterday && c.CreatedAt < today,
                cancellationToken);

            // Count resolved conversations
            var resolvedConversations = await _conversationRepository.FindAsync(
                c => c.Status == ConversationStatus.Resolved
                     && c.UpdatedAt >= yesterday && c.UpdatedAt < today,
                cancellationToken);

            // Count messages
            var messages = await _messageRepository.FindAsync(
                m => m.CreatedAt >= yesterday && m.CreatedAt < today,
                cancellationToken);

            // CSAT scores
            var csatResponses = await _csatRepository.FindAsync(
                s => s.Rating.HasValue && s.CreatedAt >= yesterday && s.CreatedAt < today,
                cancellationToken);

            var avgCsat = csatResponses.Any()
                ? csatResponses.Average(s => s.Rating!.Value)
                : 0.0;

            // Store as a reporting event
            var reportEvent = new ReportingEvent
            {
                Name = "daily_summary",
                Value = newConversations.Count,
                AccountId = 0, // Global report
                EventStartedAt = yesterday,
                EventEndedAt = today,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reportingEventRepository.AddAsync(reportEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Daily report generated for {Date}: {NewConversations} new, {Resolved} resolved, CSAT avg: {Csat}",
                yesterday.ToString("yyyy-MM-dd"), newConversations.Count, resolvedConversations.Count, avgCsat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily report for {Date}", yesterday.ToString("yyyy-MM-dd"));
        }
    }
}
