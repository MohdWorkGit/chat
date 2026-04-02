using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/conversations/{conversationId:int}/drafts")]
[Authorize]
public class DraftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DraftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetDraft(int accountId, int conversationId)
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Drafts.Queries.GetDraftQuery(conversationId, accountId, userId));
        if (result is null)
            return NoContent();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> SaveDraft(int accountId, int conversationId, [FromBody] SaveDraftRequest request)
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Drafts.Commands.SaveDraftCommand(conversationId, accountId, userId, request.Content, request.ContentType));
        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteDraft(int accountId, int conversationId)
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        await _mediator.Send(
            new Application.Drafts.Commands.DeleteDraftCommand(conversationId, accountId, userId));
        return NoContent();
    }
}

public record SaveDraftRequest(string Content, string ContentType = "text");
