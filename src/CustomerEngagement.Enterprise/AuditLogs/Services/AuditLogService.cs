using System.Text.Json;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Enterprise.AuditLogs.DTOs;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IRepository<AuditLog> auditLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(
        int accountId,
        int userId,
        string action,
        string auditableType,
        int auditableId,
        object? changes = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            AccountId = accountId,
            UserId = userId,
            Action = action,
            AuditableType = auditableType,
            AuditableId = auditableId,
            Changes = changes is not null ? JsonSerializer.Serialize(changes) : null,
            IpAddress = ipAddress
        };

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created: {Action} on {AuditableType} {AuditableId} by user {UserId} in account {AccountId}",
            action, auditableType, auditableId, userId, accountId);
    }

    public async Task<PaginatedResultDto<AuditLogDto>> GetLogsAsync(
        int accountId,
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _auditLogRepository.QueryNoTracking()
            .Where(a => a.AccountId == accountId);

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrWhiteSpace(filter.AuditableType))
            query = query.Where(a => a.AuditableType == filter.AuditableType);

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);

        var totalCount = query.Count();

        var items = query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.AccountId,
                a.UserId,
                a.UserName,
                a.Action,
                a.AuditableType,
                a.AuditableId,
                a.Changes,
                a.IpAddress,
                a.UserAgent,
                a.CreatedAt))
            .ToList();

        return new PaginatedResultDto<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
