using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Reporting;

public class ReportBuilder : IReportBuilder
{
    private readonly IRepository<ReportingEvent> _reportingEventRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly ILogger<ReportBuilder> _logger;

    public ReportBuilder(
        IRepository<ReportingEvent> reportingEventRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        ILogger<ReportBuilder> logger)
    {
        _reportingEventRepository = reportingEventRepository ?? throw new ArgumentNullException(nameof(reportingEventRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReportDto> GetConversationReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.ListAsync(
            new { AccountId = accountId, EventName = "conversation", Since = filter.Since, Until = filter.Until },
            cancellationToken);

        return BuildTimeSeriesReport("Conversations", events, filter);
    }

    public async Task<ReportDto> GetAgentReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.ListAsync(
            new { AccountId = accountId, EventName = "agent_activity", Since = filter.Since, Until = filter.Until, AgentId = filter.AgentId },
            cancellationToken);

        return BuildTimeSeriesReport("Agent Activity", events, filter);
    }

    public async Task<ReportDto> GetInboxReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.ListAsync(
            new { AccountId = accountId, EventName = "inbox_activity", Since = filter.Since, Until = filter.Until, InboxId = filter.InboxId },
            cancellationToken);

        return BuildTimeSeriesReport("Inbox Activity", events, filter);
    }

    public async Task<ReportDto> GetTeamReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.ListAsync(
            new { AccountId = accountId, EventName = "team_activity", Since = filter.Since, Until = filter.Until, TeamId = filter.TeamId },
            cancellationToken);

        return BuildTimeSeriesReport("Team Activity", events, filter);
    }

    public async Task<ReportDto> GetLabelReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.ListAsync(
            new { AccountId = accountId, EventName = "label_activity", Since = filter.Since, Until = filter.Until, LabelName = filter.LabelName },
            cancellationToken);

        return BuildTimeSeriesReport("Label Activity", events, filter);
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.ListAsync(
            new { AccountId = accountId, Since = filter.Since, Until = filter.Until },
            cancellationToken);

        var conversationList = conversations.ToList();

        var messages = await _messageRepository.ListAsync(
            new { AccountId = accountId, Since = filter.Since, Until = filter.Until },
            cancellationToken);

        var messageList = messages.ToList();

        return new ReportSummaryDto
        {
            TotalConversations = conversationList.Count,
            ResolvedConversations = conversationList.Count(c => c.Status == 2),
            OpenConversations = conversationList.Count(c => c.Status == 0),
            PendingConversations = conversationList.Count(c => c.Status == 1),
            TotalMessages = messageList.Count,
            TotalIncomingMessages = messageList.Count(m => m.MessageType == 0),
            TotalOutgoingMessages = messageList.Count(m => m.MessageType == 1)
        };
    }

    private static ReportDto BuildTimeSeriesReport(string reportName, IEnumerable<ReportingEvent> events, ReportFilterDto filter)
    {
        var eventList = events.ToList();
        var dataPoints = new List<ReportDataPointDto>();

        var currentDate = filter.Since.Date;
        while (currentDate <= filter.Until.Date)
        {
            var nextDate = filter.GroupBy.ToLowerInvariant() switch
            {
                "week" => currentDate.AddDays(7),
                "month" => currentDate.AddMonths(1),
                _ => currentDate.AddDays(1)
            };

            var periodEvents = eventList.Where(e => e.CreatedAt >= currentDate && e.CreatedAt < nextDate).ToList();

            dataPoints.Add(new ReportDataPointDto
            {
                Date = currentDate,
                Value = periodEvents.Count,
                Label = currentDate.ToString("yyyy-MM-dd")
            });

            currentDate = nextDate;
        }

        return new ReportDto
        {
            ReportName = reportName,
            Since = filter.Since,
            Until = filter.Until,
            GroupBy = filter.GroupBy,
            DataPoints = dataPoints,
            Total = eventList.Count
        };
    }
}
