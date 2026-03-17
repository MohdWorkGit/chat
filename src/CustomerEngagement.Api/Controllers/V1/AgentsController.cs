using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(new Application.Agents.Queries.GetAgentsQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{agentId:long}")]
    public async Task<ActionResult> GetById(long accountId, long agentId)
    {
        var result = await _mediator.Send(new Application.Agents.Queries.GetAgentByIdQuery(accountId, agentId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Agents.Commands.CreateAgentCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, agentId = result }, new { Id = result });
    }

    [HttpPut("{agentId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long agentId,
        [FromBody] Application.Agents.Commands.UpdateAgentCommand command)
    {
        command = command with { AccountId = accountId, Id = agentId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{agentId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long agentId)
    {
        await _mediator.Send(new Application.Agents.Commands.DeleteAgentCommand(accountId, agentId));
        return NoContent();
    }
}
