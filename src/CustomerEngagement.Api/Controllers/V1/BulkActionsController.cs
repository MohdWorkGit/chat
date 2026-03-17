using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/bulk_actions")]
[Authorize]
public class BulkActionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BulkActionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("assign")]
    public async Task<ActionResult> BulkAssign(long accountId,
        [FromBody] Application.BulkActions.Commands.BulkAssignCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("label")]
    public async Task<ActionResult> BulkLabel(long accountId,
        [FromBody] Application.BulkActions.Commands.BulkLabelCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("resolve")]
    public async Task<ActionResult> BulkResolve(long accountId,
        [FromBody] Application.BulkActions.Commands.BulkResolveCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
