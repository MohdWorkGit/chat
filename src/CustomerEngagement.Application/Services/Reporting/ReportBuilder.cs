using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
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
        var events = await _reportingEventRepository.FindAsync(
            e => e.AccountId == accountId
                 && e.Name == "conversation"
                 && e.CreatedAt >= filter.Since
                 && e.CreatedAt <= filter.Until,
            cancellationToken);

        return BuildTimeSeriesReport("Conversations", events, filter);
    }

    public async Task<ReportDto> GetAgentReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.FindAsync(
            e => e.AccountId == accountId
                 && e.Name == "agent_activity"
                 && e.CreatedAt >= filter.Since
                 && e.CreatedAt <= filter.Until
                 && (filter.AgentId == null || e.UserId == filter.AgentId),
            cancellationToken);

        return BuildTimeSeriesReport("Agent Activity", events, filter);
    }

    public async Task<ReportDto> GetInboxReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.FindAsync(
            e => e.AccountId == accountId
                 && e.Name == "inbox_activity"
                 && e.CreatedAt >= filter.Since
                 && e.CreatedAt <= filter.Until
                 && (filter.InboxId == null || e.InboxId == filter.InboxId),
            cancellationToken);

        return BuildTimeSeriesReport("Inbox Activity", events, filter);
    }

    public async Task<ReportDto> GetTeamReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.FindAsync(
            e => e.AccountId == accountId
                 && e.Name == "team_activity"
                 && e.CreatedAt >= filter.Since
                 && e.CreatedAt <= filter.Until,
            cancellationToken);

        return BuildTimeSeriesReport("Team Activity", events, filter);
    }

    public async Task<ReportDto> GetLabelReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var events = await _reportingEventRepository.FindAsync(
            e => e.AccountId == accountId
                 && e.Name == "label_activity"
                 && e.CreatedAt >= filter.Since
                 && e.CreatedAt <= filter.Until,
            cancellationToken);

        return BuildTimeSeriesReport("Label Activity", events, filter);
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.FindAsync(
            c => c.AccountId == accountId
                 && c.CreatedAt >= filter.Since
                 && c.CreatedAt <= filter.Until,
            cancellationToken);

        var conversationList = conversations.ToList();

        var messages = await _messageRepository.FindAsync(
            m => m.AccountId == accountId
                 && m.CreatedAt >= filter.Since
                 && m.CreatedAt <= filter.Until,
            cancellationToken);

        var messageList = messages.ToList();

        return new ReportSummaryDto
        {
            TotalConversations = conversationList.Count,
            ResolvedConversations = conversationList.Count(c => c.Status == ConversationStatus.Resolved),
            OpenConversations = conversationList.Count(c => c.Status == ConversationStatus.Open),
            PendingConversations = conversationList.Count(c => c.Status == ConversationStatus.Pending),
            TotalMessages = messageList.Count,
            TotalIncomingMessages = messageList.Count(m => m.MessageType == MessageType.Incoming),
            TotalOutgoingMessages = messageList.Count(m => m.MessageType == MessageType.Outgoing)
        };
    }

    public async Task<object> GetTrafficReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.FindAsync(
            c => c.AccountId == accountId
                 && c.CreatedAt >= filter.Since
                 && c.CreatedAt <= filter.Until,
            cancellationToken);

        // Aggregate conversation counts by (dayOfWeek, hour)
        var cells = conversations
            .GroupBy(c => new { Day = (int)c.CreatedAt.DayOfWeek, Hour = c.CreatedAt.Hour })
            .Select(g => new { day = g.Key.Day, hour = g.Key.Hour, value = g.Count() })
            .ToList();

        return new { cells };
    }

    public async Task<object> GetBotMetricsAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Bot replies are recorded as messages with SenderType = "AgentBot"
        var botMessages = await _messageRepository.FindAsync(
            m => m.AccountId == accountId
                 && m.SenderType == "AgentBot"
                 && m.CreatedAt >= filter.Since
                 && m.CreatedAt <= filter.Until,
            cancellationToken);

        // Unique conversations touched by the bot
        var botConversationIds = botMessages.Select(m => m.ConversationId).Distinct().ToHashSet();
        int total = botConversationIds.Count;

        // A conversation was "resolved by bot" if it was resolved and the last outgoing message was from the bot
        var allConversations = await _conversationRepository.FindAsync(
            c => c.AccountId == accountId
                 && botConversationIds.Contains(c.Id)
                 && c.CreatedAt >= filter.Since,
            cancellationToken);

        int resolvedByBot = allConversations.Count(c => c.Status == CustomerEngagement.Core.Enums.ConversationStatus.Resolved);
        int handoffs = total - resolvedByBot;
        int resolutionRate = total > 0 ? (int)Math.Round((double)resolvedByBot / total * 100) : 0;

        return new
        {
            totalConversations = total,
            resolvedByBot,
            handoffs,
            resolutionRate,
            byInbox = Array.Empty<object>()
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
