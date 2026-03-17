using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/webhooks")]
[Authorize(Policy = "Administrator")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId)
    {
        var result = await _mediator.Send(new Application.Webhooks.Queries.GetWebhooksQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{webhookId:long}")]
    public async Task<ActionResult> GetById(long accountId, long webhookId)
    {
        var result = await _mediator.Send(
            new Application.Webhooks.Queries.GetWebhookByIdQuery(accountId, webhookId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Webhooks.Commands.CreateWebhookCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, webhookId = result }, new { Id = result });
    }

    [HttpPut("{webhookId:long}")]
    public async Task<ActionResult> Update(long accountId, long webhookId,
        [FromBody] Application.Webhooks.Commands.UpdateWebhookCommand command)
    {
        command = command with { AccountId = accountId, Id = webhookId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{webhookId:long}")]
    public async Task<ActionResult> Delete(long accountId, long webhookId)
    {
        await _mediator.Send(new Application.Webhooks.Commands.DeleteWebhookCommand(accountId, webhookId));
        return NoContent();
    }
}
