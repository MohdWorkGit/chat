using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Widget;

[ApiController]
[Route("api/v1/widget/config")]
public class WidgetConfigController : ControllerBase
{
    private readonly IMediator _mediator;

    public WidgetConfigController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetConfig(
        [FromHeader(Name = "X-Website-Token")] string websiteToken)
    {
        var result = await _mediator.Send(
            new Application.Widget.Queries.GetWidgetConfigQuery(websiteToken));
        return Ok(result);
    }
}
