using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/csat_survey")]
[Authorize]
public class CsatSurveyController : ControllerBase
{
    private readonly IMediator _mediator;

    public CsatSurveyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId,
        [FromQuery] DateTime? since, [FromQuery] DateTime? until,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.CsatSurvey.Queries.GetCsatResponsesQuery(accountId, since, until, page, pageSize));
        return Ok(result);
    }

    [HttpGet("metrics")]
    public async Task<ActionResult> GetMetrics(long accountId,
        [FromQuery] DateTime? since, [FromQuery] DateTime? until)
    {
        var result = await _mediator.Send(
            new Application.CsatSurvey.Queries.GetCsatMetricsQuery(accountId, since, until));
        return Ok(result);
    }
}
