using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/inboxes")]
[Authorize]
public class InboxesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InboxesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(new Application.Inboxes.Queries.GetInboxesQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{inboxId:long}")]
    public async Task<ActionResult> GetById(long accountId, long inboxId)
    {
        var result = await _mediator.Send(new Application.Inboxes.Queries.GetInboxByIdQuery(accountId, inboxId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Inboxes.Commands.CreateInboxCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, inboxId = result }, new { Id = result });
    }

    [HttpPut("{inboxId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long inboxId,
        [FromBody] Application.Inboxes.Commands.UpdateInboxCommand command)
    {
        command = command with { AccountId = accountId, Id = inboxId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{inboxId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long inboxId)
    {
        await _mediator.Send(new Application.Inboxes.Commands.DeleteInboxCommand(accountId, inboxId));
        return NoContent();
    }

    [HttpGet("{inboxId:long}/members")]
    public async Task<ActionResult> GetMembers(long accountId, long inboxId)
    {
        var result = await _mediator.Send(
            new Application.Inboxes.Queries.GetInboxMembersQuery(accountId, inboxId));
        return Ok(result);
    }

    [HttpPost("{inboxId:long}/members")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> AddMember(long accountId, long inboxId,
        [FromBody] Application.Inboxes.Commands.AddInboxMemberCommand command)
    {
        command = command with { AccountId = accountId, InboxId = inboxId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpDelete("{inboxId:long}/members/{userId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> RemoveMember(long accountId, long inboxId, long userId)
    {
        await _mediator.Send(
            new Application.Inboxes.Commands.RemoveInboxMemberCommand(accountId, inboxId, userId));
        return NoContent();
    }

    [HttpGet("{inboxId:long}/working_hours")]
    public async Task<ActionResult> GetWorkingHours(long accountId, long inboxId)
    {
        var result = await _mediator.Send(
            new Application.Inboxes.Queries.GetWorkingHoursQuery(accountId, inboxId));
        return Ok(result);
    }

    [HttpGet("{inboxId:long}/widget_config")]
    public async Task<ActionResult> GetWidgetConfig(long accountId, long inboxId)
    {
        var result = await _mediator.Send(
            new Application.Inboxes.Queries.GetInboxWidgetConfigQuery(accountId, inboxId));
        return Ok(result);
    }

    [HttpPut("{inboxId:long}/working_hours")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> UpdateWorkingHours(long accountId, long inboxId,
        [FromBody] Application.Inboxes.Commands.UpdateWorkingHoursCommand command)
    {
        command = command with { AccountId = accountId, InboxId = inboxId };
        await _mediator.Send(command);
        return NoContent();
    }
}
