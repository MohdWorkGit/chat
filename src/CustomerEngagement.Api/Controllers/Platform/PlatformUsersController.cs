using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Platform;

[ApiController]
[Route("api/v1/platform/users")]
[Authorize(Policy = "Administrator")]
public class PlatformUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Platform.Queries.GetPlatformUsersQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{userId:long}")]
    public async Task<ActionResult> GetById(long userId)
    {
        var result = await _mediator.Send(new Application.Platform.Queries.GetPlatformUserByIdQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] Application.Platform.Commands.CreatePlatformUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { userId = result }, new { Id = result });
    }

    [HttpPut("{userId:long}")]
    public async Task<ActionResult> Update(long userId,
        [FromBody] Application.Platform.Commands.UpdatePlatformUserCommand command)
    {
        command = command with { Id = userId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{userId:long}")]
    public async Task<ActionResult> Delete(long userId)
    {
        await _mediator.Send(new Application.Platform.Commands.DeletePlatformUserCommand(userId));
        return NoContent();
    }
}
