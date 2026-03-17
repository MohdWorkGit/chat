using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Contacts.Queries.GetContactsQuery(accountId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{contactId:long}")]
    public async Task<ActionResult> GetById(long accountId, long contactId)
    {
        var result = await _mediator.Send(
            new Application.Contacts.Queries.GetContactByIdQuery(accountId, contactId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Contacts.Commands.CreateContactCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, contactId = result }, new { Id = result });
    }

    [HttpPut("{contactId:long}")]
    public async Task<ActionResult> Update(long accountId, long contactId,
        [FromBody] Application.Contacts.Commands.UpdateContactCommand command)
    {
        command = command with { AccountId = accountId, Id = contactId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{contactId:long}")]
    public async Task<ActionResult> Delete(long accountId, long contactId)
    {
        await _mediator.Send(new Application.Contacts.Commands.DeleteContactCommand(accountId, contactId));
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult> Search(long accountId, [FromQuery] string q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Contacts.Queries.SearchContactsQuery(accountId, q, page, pageSize));
        return Ok(result);
    }

    [HttpPost("merge")]
    public async Task<ActionResult> Merge(long accountId,
        [FromBody] Application.Contacts.Commands.MergeContactsCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("import")]
    public async Task<ActionResult> Import(long accountId, IFormFile file)
    {
        var result = await _mediator.Send(
            new Application.Contacts.Commands.ImportContactsCommand(accountId, file));
        return Ok(result);
    }
}
