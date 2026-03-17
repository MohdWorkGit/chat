using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetCurrentProfile()
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(new Application.Profiles.Queries.GetProfileQuery(userId));
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateProfile(
        [FromBody] Application.Profiles.Commands.UpdateProfileCommand command)
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        command = command with { UserId = userId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("availability")]
    public async Task<ActionResult> UpdateAvailability(
        [FromBody] Application.Profiles.Commands.UpdateAvailabilityCommand command)
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        command = command with { UserId = userId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UpdateAvatar(IFormFile file)
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Profiles.Commands.UpdateAvatarCommand(userId, file));
        return Ok(result);
    }

    [HttpDelete("avatar")]
    public async Task<ActionResult> DeleteAvatar()
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        await _mediator.Send(new Application.Profiles.Commands.DeleteAvatarCommand(userId));
        return NoContent();
    }
}
