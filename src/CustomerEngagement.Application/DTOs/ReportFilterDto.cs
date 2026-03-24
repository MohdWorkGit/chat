namespace CustomerEngagement.Application.DTOs;

public class ReportFilterDto
{
    public DateTime Since { get; set; }
    public DateTime Until { get; set; }
    public int? AgentId { get; set; }
    public int? InboxId { get; set; }
    public int? TeamId { get; set; }
    public string? LabelName { get; set; }
    public string GroupBy { get; set; } = "day"; // day, week, month
}

public class ReportSummaryDto
{
    public int TotalConversations { get; set; }
    public int ResolvedConversations { get; set; }
    public int OpenConversations { get; set; }
    public int PendingConversations { get; set; }
    public double AverageFirstResponseTimeSeconds { get; set; }
    public double AverageResolutionTimeSeconds { get; set; }
    public int TotalMessages { get; set; }
    public int TotalIncomingMessages { get; set; }
    public int TotalOutgoingMessages { get; set; }
}
