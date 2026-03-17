using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.SuperAdmin;

[ApiController]
[Route("api/v1/super_admin/users")]
[Authorize(Roles = "SuperAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.SuperAdmin.Queries.GetAdminUsersQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{userId:long}")]
    public async Task<ActionResult> GetById(long userId)
    {
        var result = await _mediator.Send(
            new Application.SuperAdmin.Queries.GetAdminUserByIdQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] Application.SuperAdmin.Commands.CreateAdminUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { userId = result }, new { Id = result });
    }

    [HttpPut("{userId:long}")]
    public async Task<ActionResult> Update(long userId,
        [FromBody] Application.SuperAdmin.Commands.UpdateAdminUserCommand command)
    {
        command = command with { Id = userId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{userId:long}")]
    public async Task<ActionResult> Delete(long userId)
    {
        await _mediator.Send(new Application.SuperAdmin.Commands.DeleteAdminUserCommand(userId));
        return NoContent();
    }
}
