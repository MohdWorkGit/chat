using CustomerEngagement.Enterprise.Captain.DTOs;
using CustomerEngagement.Enterprise.Captain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/copilot")]
[Authorize]
public class CopilotController : ControllerBase
{
    private readonly ICopilotService _copilotService;

    public CopilotController(ICopilotService copilotService)
    {
        _copilotService = copilotService;
    }

    [HttpPost("suggest")]
    public async Task<ActionResult<CopilotSuggestion>> SuggestReply(
        int accountId,
        [FromBody] SuggestRequest request,
        CancellationToken cancellationToken)
    {
        var suggestion = await _copilotService.SuggestReplyAsync(
            request.ConversationId, cancellationToken);

        return Ok(suggestion);
    }

    [HttpPost("rewrite")]
    public async Task<ActionResult<RewriteResult>> Rewrite(
        int accountId,
        [FromBody] RewriteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _copilotService.RewriteAsync(
            request.Text, request.Tone, cancellationToken);

        return Ok(result);
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<ConversationSummary>> Summarize(
        int accountId,
        [FromBody] SummarizeRequest request,
        CancellationToken cancellationToken)
    {
        var summary = await _copilotService.SummarizeAsync(
            request.ConversationId, cancellationToken);

        return Ok(summary);
    }

    [HttpPost("suggest-labels")]
    public async Task<ActionResult<IReadOnlyList<string>>> SuggestLabels(
        int accountId,
        [FromBody] SuggestRequest request,
        CancellationToken cancellationToken)
    {
        var labels = await _copilotService.SuggestLabelsAsync(
            request.ConversationId, cancellationToken);

        return Ok(labels);
    }

    [HttpPost("suggest-follow-up")]
    public async Task<ActionResult<IReadOnlyList<string>>> SuggestFollowUp(
        int accountId,
        [FromBody] SuggestRequest request,
        CancellationToken cancellationToken)
    {
        var followUps = await _copilotService.SuggestFollowUpAsync(
            request.ConversationId, cancellationToken);

        return Ok(followUps);
    }

    public sealed record SuggestRequest(int ConversationId);
    public sealed record RewriteRequest(string Text, string Tone = "professional");
    public sealed record SummarizeRequest(int ConversationId);
}
