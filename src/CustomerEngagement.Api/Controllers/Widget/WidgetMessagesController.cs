using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Widget;

[ApiController]
[Route("api/v1/widget/conversations/{conversationId:long}/messages")]
public class WidgetMessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WidgetMessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetMessages(long conversationId,
        [FromHeader(Name = "X-Widget-Token")] string widgetToken,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(
            new Application.Widget.Queries.GetWidgetMessagesQuery(widgetToken, conversationId, page, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> SendMessage(long conversationId,
        [FromHeader(Name = "X-Widget-Token")] string widgetToken,
        [FromBody] Application.Widget.Commands.SendWidgetMessageCommand command)
    {
        command = command with { WidgetToken = widgetToken, ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return Ok(new { Id = result });
    }
}
