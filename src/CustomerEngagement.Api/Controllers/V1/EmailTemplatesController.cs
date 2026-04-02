using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/email_templates")]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailTemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(int accountId)
    {
        var result = await _mediator.Send(
            new Application.EmailTemplates.Queries.GetEmailTemplatesQuery(accountId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int accountId, int id)
    {
        var result = await _mediator.Send(
            new Application.EmailTemplates.Queries.GetEmailTemplateByIdQuery(id, accountId));
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(int accountId, [FromBody] CreateEmailTemplateRequest request)
    {
        var result = await _mediator.Send(
            new Application.EmailTemplates.Commands.CreateEmailTemplateCommand(
                accountId, request.Name, request.Body, request.TemplateType, request.Locale));
        return CreatedAtAction(nameof(GetById), new { accountId, id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int accountId, int id, [FromBody] UpdateEmailTemplateRequest request)
    {
        var result = await _mediator.Send(
            new Application.EmailTemplates.Commands.UpdateEmailTemplateCommand(
                id, accountId, request.Name, request.Body, request.TemplateType, request.Locale));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int accountId, int id)
    {
        await _mediator.Send(new Application.EmailTemplates.Commands.DeleteEmailTemplateCommand(id, accountId));
        return NoContent();
    }

    [HttpPost("{id:int}/render")]
    public async Task<ActionResult> Render(int accountId, int id, [FromBody] RenderTemplateRequest request)
    {
        var rendered = await _mediator.Send(
            new Application.EmailTemplates.Queries.RenderEmailTemplateQuery(id, accountId, request.Variables));
        return Ok(new { rendered });
    }
}

public record CreateEmailTemplateRequest(string Name, string? Body, string? TemplateType, string? Locale);
public record UpdateEmailTemplateRequest(string Name, string? Body, string? TemplateType, string? Locale);
public record RenderTemplateRequest(Dictionary<string, object> Variables);
