using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Platform;

[ApiController]
[Route("api/v1/platform/accounts")]
[Authorize(Policy = "Administrator")]
public class PlatformAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Platform.Queries.GetPlatformAccountsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{accountId:long}")]
    public async Task<ActionResult> GetById(long accountId)
    {
        var result = await _mediator.Send(
            new Application.Platform.Queries.GetPlatformAccountByIdQuery(accountId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] Application.Platform.Commands.CreatePlatformAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId = result }, new { Id = result });
    }

    [HttpPut("{accountId:long}")]
    public async Task<ActionResult> Update(long accountId,
        [FromBody] Application.Platform.Commands.UpdatePlatformAccountCommand command)
    {
        command = command with { Id = accountId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{accountId:long}")]
    public async Task<ActionResult> Delete(long accountId)
    {
        await _mediator.Send(new Application.Platform.Commands.DeletePlatformAccountCommand(accountId));
        return NoContent();
    }
}
