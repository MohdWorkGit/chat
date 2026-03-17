using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/reports")]
[Authorize(Policy = "Administrator")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult> ConversationReport(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until,
        [FromQuery] string? groupBy)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetConversationReportQuery(accountId, since, until, groupBy));
        return Ok(result);
    }

    [HttpGet("agents")]
    public async Task<ActionResult> AgentReport(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetAgentReportQuery(accountId, since, until));
        return Ok(result);
    }

    [HttpGet("inboxes")]
    public async Task<ActionResult> InboxReport(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetInboxReportQuery(accountId, since, until));
        return Ok(result);
    }

    [HttpGet("teams")]
    public async Task<ActionResult> TeamReport(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetTeamReportQuery(accountId, since, until));
        return Ok(result);
    }

    [HttpGet("labels")]
    public async Task<ActionResult> LabelReport(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetLabelReportQuery(accountId, since, until));
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult> Summary(long accountId,
        [FromQuery] DateTime since, [FromQuery] DateTime until)
    {
        var result = await _mediator.Send(
            new Application.Reports.Queries.GetSummaryReportQuery(accountId, since, until));
        return Ok(result);
    }
}
