using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Widget;

[ApiController]
[Route("api/v1/widget/contacts")]
public class WidgetContactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WidgetContactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromHeader(Name = "X-Website-Token")] string websiteToken,
        [FromBody] Application.Widget.Commands.CreateWidgetContactCommand command)
    {
        command = command with { WidgetToken = websiteToken };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch]
    public async Task<ActionResult> Update(
        [FromHeader(Name = "X-Website-Token")] string websiteToken,
        [FromHeader(Name = "X-Contact-Identifier")] string contactIdentifier,
        [FromBody] Application.Widget.Commands.UpdateWidgetContactCommand command)
    {
        command = command with { WidgetToken = websiteToken, ContactIdentifier = contactIdentifier };
        await _mediator.Send(command);
        return Ok();
    }
}
