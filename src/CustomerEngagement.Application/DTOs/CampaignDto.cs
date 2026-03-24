namespace CustomerEngagement.Application.DTOs;

public class CreateCampaignRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CampaignType { get; set; }
    public int? InboxId { get; set; }
    public string? Audience { get; set; }
    public string? ScheduledAt { get; set; }
}

public class UpdateCampaignRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Message { get; set; }
    public string? Audience { get; set; }
    public string? ScheduledAt { get; set; }
}
