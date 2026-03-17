using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Platform;

[ApiController]
[Route("api/v1/platform/agent_bots")]
[Authorize(Policy = "Administrator")]
public class PlatformAgentBotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformAgentBotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Platform.Queries.GetAgentBotsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{botId:long}")]
    public async Task<ActionResult> GetById(long botId)
    {
        var result = await _mediator.Send(new Application.Platform.Queries.GetAgentBotByIdQuery(botId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] Application.Platform.Commands.CreateAgentBotCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { botId = result }, new { Id = result });
    }

    [HttpPut("{botId:long}")]
    public async Task<ActionResult> Update(long botId,
        [FromBody] Application.Platform.Commands.UpdateAgentBotCommand command)
    {
        command = command with { Id = botId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{botId:long}")]
    public async Task<ActionResult> Delete(long botId)
    {
        await _mediator.Send(new Application.Platform.Commands.DeleteAgentBotCommand(botId));
        return NoContent();
    }
}
