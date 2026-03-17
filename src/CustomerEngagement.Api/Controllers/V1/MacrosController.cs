using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/macros")]
[Authorize]
public class MacrosController : ControllerBase
{
    private readonly IMediator _mediator;

    public MacrosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new Application.Macros.Queries.GetMacrosQuery(accountId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{macroId:long}")]
    public async Task<ActionResult> GetById(long accountId, long macroId)
    {
        var result = await _mediator.Send(new Application.Macros.Queries.GetMacroByIdQuery(accountId, macroId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Macros.Commands.CreateMacroCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, macroId = result }, new { Id = result });
    }

    [HttpPut("{macroId:long}")]
    public async Task<ActionResult> Update(long accountId, long macroId,
        [FromBody] Application.Macros.Commands.UpdateMacroCommand command)
    {
        command = command with { AccountId = accountId, Id = macroId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{macroId:long}")]
    public async Task<ActionResult> Delete(long accountId, long macroId)
    {
        await _mediator.Send(new Application.Macros.Commands.DeleteMacroCommand(accountId, macroId));
        return NoContent();
    }

    [HttpPost("{macroId:long}/execute")]
    public async Task<ActionResult> Execute(long accountId, long macroId,
        [FromBody] Application.Macros.Commands.ExecuteMacroCommand command)
    {
        command = command with { AccountId = accountId, MacroId = macroId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
