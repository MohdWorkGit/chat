using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new Application.Accounts.Queries.GetAccountsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{accountId:long}")]
    public async Task<ActionResult> GetById(long accountId)
    {
        var result = await _mediator.Send(new Application.Accounts.Queries.GetAccountByIdQuery(accountId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create([FromBody] Application.Accounts.Commands.CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId = result }, new { Id = result });
    }

    [HttpPut("{accountId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, [FromBody] Application.Accounts.Commands.UpdateAccountCommand command)
    {
        command = command with { Id = accountId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{accountId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId)
    {
        await _mediator.Send(new Application.Accounts.Commands.DeleteAccountCommand(accountId));
        return NoContent();
    }
}
