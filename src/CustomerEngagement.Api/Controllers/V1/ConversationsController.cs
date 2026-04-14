using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] string? status, [FromQuery] long? inboxId,
        [FromQuery] long? assigneeId, [FromQuery] long? teamId, [FromQuery] string? label,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new Application.Conversations.Queries.GetConversationsQuery(
            accountId, status, inboxId, assigneeId, teamId, label, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{conversationId:long}")]
    public async Task<ActionResult> GetById(long accountId, long conversationId)
    {
        var result = await _mediator.Send(
            new Application.Conversations.Queries.GetConversationByIdQuery(accountId, conversationId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Conversations.Commands.CreateConversationCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, conversationId = result }, new { Id = result });
    }

    [HttpPatch("{conversationId:long}")]
    public async Task<ActionResult> Update(long accountId, long conversationId,
        [FromBody] Application.Conversations.Commands.UpdateConversationCommand command)
    {
        command = command with { AccountId = accountId, Id = conversationId };
        await _mediator.Send(command);
        var updated = await _mediator.Send(
            new Application.Conversations.Queries.GetConversationByIdQuery(accountId, conversationId));
        return Ok(updated);
    }

    [HttpPost("{conversationId:long}/resolve")]
    public async Task<ActionResult> Resolve(long accountId, long conversationId)
    {
        await _mediator.Send(new Application.Conversations.Commands.ResolveConversationCommand(accountId, conversationId));
        return Ok();
    }

    [HttpPost("{conversationId:long}/reopen")]
    public async Task<ActionResult> Reopen(long accountId, long conversationId)
    {
        await _mediator.Send(new Application.Conversations.Commands.ReopenConversationCommand(accountId, conversationId));
        return Ok();
    }

    [HttpPost("{conversationId:long}/mute")]
    public async Task<ActionResult> Mute(long accountId, long conversationId)
    {
        await _mediator.Send(new Application.Conversations.Commands.MuteConversationCommand(accountId, conversationId));
        return Ok();
    }

    [HttpPost("{conversationId:long}/unmute")]
    public async Task<ActionResult> Unmute(long accountId, long conversationId)
    {
        await _mediator.Send(new Application.Conversations.Commands.UnmuteConversationCommand(accountId, conversationId));
        return Ok();
    }

    [HttpPost("{conversationId:long}/snooze")]
    public async Task<ActionResult> Snooze(long accountId, long conversationId,
        [FromBody] Application.Conversations.Commands.SnoozeConversationCommand command)
    {
        command = command with { AccountId = accountId, ConversationId = conversationId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("{conversationId:long}/toggle_priority")]
    public async Task<ActionResult> TogglePriority(long accountId, long conversationId)
    {
        await _mediator.Send(
            new Application.Conversations.Commands.TogglePriorityCommand(accountId, conversationId));
        return Ok();
    }

    [HttpPost("{conversationId:long}/assignments")]
    public async Task<ActionResult> Assign(long accountId, long conversationId,
        [FromBody] Application.Conversations.Commands.AssignConversationCommand command)
    {
        command = command with { AccountId = accountId, ConversationId = conversationId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("{conversationId:long}/participants")]
    public async Task<ActionResult> GetParticipants(long accountId, long conversationId)
    {
        var result = await _mediator.Send(
            new Application.Conversations.Queries.GetParticipantsQuery(accountId, conversationId));
        return Ok(result);
    }

    [HttpPost("{conversationId:long}/participants")]
    public async Task<ActionResult> AddParticipant(long accountId, long conversationId,
        [FromBody] Application.Conversations.Commands.AddParticipantCommand command)
    {
        command = command with { AccountId = accountId, ConversationId = conversationId };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpDelete("{conversationId:long}/participants/{userId:long}")]
    public async Task<ActionResult> RemoveParticipant(long accountId, long conversationId, long userId)
    {
        await _mediator.Send(
            new Application.Conversations.Commands.RemoveParticipantCommand(accountId, conversationId, userId));
        return NoContent();
    }
}
