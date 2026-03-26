using CustomerEngagement.Enterprise.AuditLogs.DTOs;
using CustomerEngagement.Enterprise.AuditLogs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Enterprise;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/audit_logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        long accountId,
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? auditableType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filter = new AuditLogFilterDto(userId, action, auditableType, dateFrom, dateTo, page, pageSize);
        var result = await _auditLogService.GetLogsAsync((int)accountId, filter);
        return Ok(result);
    }
}
