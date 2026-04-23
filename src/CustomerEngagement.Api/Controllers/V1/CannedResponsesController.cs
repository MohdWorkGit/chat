using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/canned_responses")]
[Authorize]
public class CannedResponsesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CannedResponsesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(
            new Application.CannedResponses.Queries.GetCannedResponsesQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{cannedResponseId:long}")]
    public async Task<ActionResult> GetById(long accountId, long cannedResponseId)
    {
        var result = await _mediator.Send(
            new Application.CannedResponses.Queries.GetCannedResponseByIdQuery(accountId, cannedResponseId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.CannedResponses.Commands.CreateCannedResponseCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{cannedResponseId:long}")]
    public async Task<ActionResult> Update(long accountId, long cannedResponseId,
        [FromBody] Application.CannedResponses.Commands.UpdateCannedResponseCommand command)
    {
        command = command with { AccountId = accountId, Id = cannedResponseId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{cannedResponseId:long}")]
    public async Task<ActionResult> Delete(long accountId, long cannedResponseId)
    {
        await _mediator.Send(
            new Application.CannedResponses.Commands.DeleteCannedResponseCommand(accountId, cannedResponseId));
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult> Search(long accountId, [FromQuery] string q)
    {
        var result = await _mediator.Send(
            new Application.CannedResponses.Queries.SearchCannedResponsesQuery(accountId, q));
        return Ok(result);
    }
}
