using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> Search(long accountId, [FromQuery] string q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Search.Queries.GlobalSearchQuery(accountId, q, page, pageSize));
        return Ok(result);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult> SearchConversations(long accountId, [FromQuery] string q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Search.Queries.SearchConversationsQuery(accountId, q, page, pageSize));
        return Ok(result);
    }

    [HttpGet("contacts")]
    public async Task<ActionResult> SearchContacts(long accountId, [FromQuery] string q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Search.Queries.SearchContactsQuery(accountId, q, page, pageSize));
        return Ok(result);
    }

    [HttpGet("messages")]
    public async Task<ActionResult> SearchMessages(long accountId, [FromQuery] string q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Search.Queries.SearchMessagesQuery(accountId, q, page, pageSize));
        return Ok(result);
    }
}
