using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/portals")]
[Authorize]
public class PortalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new Application.Portals.Queries.GetPortalsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{portalId:long}")]
    public async Task<ActionResult> GetById(long portalId)
    {
        var result = await _mediator.Send(new Application.Portals.Queries.GetPortalByIdQuery(portalId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Application.Portals.Commands.CreatePortalCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { portalId = result }, new { Id = result });
    }

    [HttpPut("{portalId:long}")]
    public async Task<ActionResult> Update(long portalId,
        [FromBody] Application.Portals.Commands.UpdatePortalCommand command)
    {
        command = command with { Id = portalId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{portalId:long}")]
    public async Task<ActionResult> Delete(long portalId)
    {
        await _mediator.Send(new Application.Portals.Commands.DeletePortalCommand(portalId));
        return NoContent();
    }
}
