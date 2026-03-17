using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.SuperAdmin;

[ApiController]
[Route("api/v1/super_admin/accounts")]
[Authorize(Roles = "SuperAdmin")]
public class AdminAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.SuperAdmin.Queries.GetAdminAccountsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{accountId:long}")]
    public async Task<ActionResult> GetById(long accountId)
    {
        var result = await _mediator.Send(
            new Application.SuperAdmin.Queries.GetAdminAccountByIdQuery(accountId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] Application.SuperAdmin.Commands.CreateAdminAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId = result }, new { Id = result });
    }

    [HttpPut("{accountId:long}")]
    public async Task<ActionResult> Update(long accountId,
        [FromBody] Application.SuperAdmin.Commands.UpdateAdminAccountCommand command)
    {
        command = command with { Id = accountId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{accountId:long}")]
    public async Task<ActionResult> Delete(long accountId)
    {
        await _mediator.Send(new Application.SuperAdmin.Commands.DeleteAdminAccountCommand(accountId));
        return NoContent();
    }
}
