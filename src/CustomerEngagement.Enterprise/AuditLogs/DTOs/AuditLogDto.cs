namespace CustomerEngagement.Enterprise.AuditLogs.DTOs;

public record AuditLogDto(
    int Id,
    int AccountId,
    int UserId,
    string? UserName,
    string Action,
    string AuditableType,
    int AuditableId,
    string? Changes,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);

public record AuditLogFilterDto(
    int? UserId = null,
    string? Action = null,
    string? AuditableType = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int Page = 1,
    int PageSize = 25);
