using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/labels")]
[Authorize]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(new Application.Labels.Queries.GetLabelsQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{labelId:long}")]
    public async Task<ActionResult> GetById(long accountId, long labelId)
    {
        var result = await _mediator.Send(new Application.Labels.Queries.GetLabelByIdQuery(accountId, labelId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Labels.Commands.CreateLabelCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, labelId = result }, new { Id = result });
    }

    [HttpPut("{labelId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long labelId,
        [FromBody] Application.Labels.Commands.UpdateLabelCommand command)
    {
        command = command with { AccountId = accountId, Id = labelId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{labelId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long labelId)
    {
        await _mediator.Send(new Application.Labels.Commands.DeleteLabelCommand(accountId, labelId));
        return NoContent();
    }
}
