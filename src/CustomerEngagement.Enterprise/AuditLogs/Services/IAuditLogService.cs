using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Enterprise.AuditLogs.DTOs;

namespace CustomerEngagement.Enterprise.AuditLogs.Services;

public interface IAuditLogService
{
    Task LogAsync(
        int accountId,
        int userId,
        string action,
        string auditableType,
        int auditableId,
        object? changes = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<AuditLogDto>> GetLogsAsync(
        int accountId,
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default);
}
