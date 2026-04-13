namespace CustomerEngagement.Application.DTOs;

public record MetaDto(int TotalCount, int CurrentPage, int PageSize, int TotalPages);

public record AttachmentDto(
    int Id, string FileType, string? ExternalUrl, string? Extension, string? FallbackTitle);

public record CreateAttachmentRequest(string FileType, string Url, string? Extension);

public record LabelDto(int Id, string Title, string? Description, string? Color, bool ShowOnSidebar);

public record TeamDto(int Id, string Name, string? Description, bool AllowAutoAssign, int MemberCount);

public record CannedResponseDto(int Id, string ShortCode, string Content);

public record NotificationDto(
    int Id, string? NotificationType, string? PrimaryActorType, int? PrimaryActorId,
    string? SecondaryActorType, int? SecondaryActorId,
    bool IsRead, DateTime CreatedAt);

public record CampaignDto(
    int Id, int AccountId, string Title, string? Description, string Message,
    int CampaignType, int? InboxId, bool Enabled, string? Audience,
    string? ScheduledAt, DateTime CreatedAt, DateTime UpdatedAt);

public class ReportDataPointDto
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string? Label { get; set; }
}

public class ReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public DateTime Since { get; set; }
    public DateTime Until { get; set; }
    public string? GroupBy { get; set; }
    public IReadOnlyList<ReportDataPointDto> DataPoints { get; set; } = [];
    public double Total { get; set; }
}

public record AccountDto(
    int Id, string Name, string? Locale, string? Domain,
    int AutoResolveAfterDays, DateTime CreatedAt);
