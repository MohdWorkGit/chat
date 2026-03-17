using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(new Application.Teams.Queries.GetTeamsQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{teamId:long}")]
    public async Task<ActionResult> GetById(long accountId, long teamId)
    {
        var result = await _mediator.Send(new Application.Teams.Queries.GetTeamByIdQuery(accountId, teamId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Teams.Commands.CreateTeamCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, teamId = result }, new { Id = result });
    }

    [HttpPut("{teamId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long teamId,
        [FromBody] Application.Teams.Commands.UpdateTeamCommand command)
    {
        command = command with { AccountId = accountId, Id = teamId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{teamId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long teamId)
    {
        await _mediator.Send(new Application.Teams.Commands.DeleteTeamCommand(accountId, teamId));
        return NoContent();
    }

    [HttpGet("{teamId:long}/members")]
    public async Task<ActionResult> GetMembers(long accountId, long teamId)
    {
        var result = await _mediator.Send(new Application.Teams.Queries.GetTeamMembersQuery(accountId, teamId));
        return Ok(result);
    }

    [HttpPost("{teamId:long}/members")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> AddMember(long accountId, long teamId,
        [FromBody] Application.Teams.Commands.AddTeamMemberCommand command)
    {
        command = command with { AccountId = accountId, TeamId = teamId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpDelete("{teamId:long}/members/{userId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> RemoveMember(long accountId, long teamId, long userId)
    {
        await _mediator.Send(
            new Application.Teams.Commands.RemoveTeamMemberCommand(accountId, teamId, userId));
        return NoContent();
    }
}
