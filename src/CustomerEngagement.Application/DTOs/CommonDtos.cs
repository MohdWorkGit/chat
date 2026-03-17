namespace CustomerEngagement.Application.DTOs;

public record MetaDto(int TotalCount, int Page, int PageSize, int TotalPages);

public record AttachmentDto(
    int Id, string FileType, string? ExternalUrl, string? Extension, string? FallbackTitle);

public record CreateAttachmentRequest(string FileType, string Url, string? Extension);

public record LabelDto(int Id, string Title, string? Description, string? Color, bool ShowOnSidebar);

public record TeamDto(int Id, string Name, string? Description, bool AllowAutoAssign, int MemberCount);

public record CannedResponseDto(int Id, string ShortCode, string Content);

public record ContactSummaryDto(int Id, string? Name, string? Email, string? Phone, string? Avatar);

public record NotificationDto(
    int Id, string? NotificationType, string? PrimaryActorType, int? PrimaryActorId,
    string? SecondaryActorType, int? SecondaryActorId,
    bool IsRead, DateTime CreatedAt);

public record WebhookDto(
    int Id, int AccountId, string Url, IReadOnlyList<string> SubscribedEvents, DateTime CreatedAt);

public record AutomationRuleDto(
    int Id, string Name, string? Description, string EventName,
    object? Conditions, object? Actions, bool Active);

public record MacroDto(int Id, string Name, object? Actions, string Visibility);

public record CampaignDto(
    int Id, string Title, string? Description, string? Message,
    string CampaignType, string? Audience, DateTime? ScheduledAt,
    bool Enabled, DateTime CreatedAt);

public record ReportDataPointDto(DateTime Date, double Value);

public record ReportDto(
    string ReportType, DateTime From, DateTime To,
    IReadOnlyList<ReportDataPointDto> DataPoints, double? Summary);

public record AccountDto(
    int Id, string Name, string? Locale, string? Domain,
    int AutoResolveAfterDays, DateTime CreatedAt);
