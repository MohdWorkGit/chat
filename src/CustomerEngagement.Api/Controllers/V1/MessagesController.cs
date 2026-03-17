using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/conversations/{conversationId:long}/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, long conversationId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(
            new Application.Messages.Queries.GetMessagesQuery(accountId, conversationId, page, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId, long conversationId,
        [FromBody] Application.Messages.Commands.CreateMessageCommand command)
    {
        command = command with { AccountId = accountId, ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { accountId, conversationId }, new { Id = result });
    }

    [HttpPut("{messageId:long}")]
    public async Task<ActionResult> Update(long accountId, long conversationId, long messageId,
        [FromBody] Application.Messages.Commands.UpdateMessageCommand command)
    {
        command = command with { AccountId = accountId, ConversationId = conversationId, Id = messageId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{messageId:long}")]
    public async Task<ActionResult> Delete(long accountId, long conversationId, long messageId)
    {
        await _mediator.Send(
            new Application.Messages.Commands.DeleteMessageCommand(accountId, conversationId, messageId));
        return NoContent();
    }
}
