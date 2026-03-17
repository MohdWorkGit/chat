using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Public;

[ApiController]
[Route("api/v1/public/inboxes/{inboxIdentifier}")]
public class PublicInboxesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicInboxesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetInbox(string inboxIdentifier)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicInboxQuery(inboxIdentifier));
        return Ok(result);
    }

    [HttpPost("contacts")]
    public async Task<ActionResult> CreateContact(string inboxIdentifier,
        [FromBody] Application.Public.Commands.CreatePublicContactCommand command)
    {
        command = command with { InboxIdentifier = inboxIdentifier };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<ActionResult> CreateConversation(string inboxIdentifier,
        [FromBody] Application.Public.Commands.CreatePublicConversationCommand command)
    {
        command = command with { InboxIdentifier = inboxIdentifier };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:long}/messages")]
    public async Task<ActionResult> CreateMessage(string inboxIdentifier, long conversationId,
        [FromBody] Application.Public.Commands.CreatePublicMessageCommand command)
    {
        command = command with { InboxIdentifier = inboxIdentifier, ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
