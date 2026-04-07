using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Widget;

[ApiController]
[Route("api/v1/widget/conversations")]
public class WidgetConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WidgetConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetConversations(
        [FromHeader(Name = "X-Website-Token")] string websiteToken,
        [FromHeader(Name = "X-Contact-Identifier")] string contactIdentifier)
    {
        var result = await _mediator.Send(
            new Application.Widget.Queries.GetWidgetConversationsQuery(websiteToken, contactIdentifier));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateConversation(
        [FromHeader(Name = "X-Website-Token")] string websiteToken,
        [FromBody] Application.Widget.Commands.CreateWidgetConversationCommand command)
    {
        command = command with { WidgetToken = websiteToken };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetConversations), new { }, new { Id = result });
    }
}
