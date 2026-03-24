using CustomerEngagement.Enterprise.Captain.DTOs;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/assistants")]
[Authorize]
public class CaptainAssistantsController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly IAssistantChatService _chatService;

    public CaptainAssistantsController(DbContext dbContext, IAssistantChatService chatService)
    {
        _dbContext = dbContext;
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptainAssistant>>> GetAll(
        int accountId,
        CancellationToken cancellationToken)
    {
        var assistants = await _dbContext.Set<CaptainAssistant>()
            .Where(a => a.AccountId == accountId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(assistants);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CaptainAssistant>> GetById(
        int accountId,
        int id,
        CancellationToken cancellationToken)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.AccountId == accountId, cancellationToken);

        if (assistant is null) return NotFound();

        return Ok(assistant);
    }

    [HttpPost]
    public async Task<ActionResult<CaptainAssistant>> Create(
        int accountId,
        [FromBody] CreateAssistantRequest request,
        CancellationToken cancellationToken)
    {
        var assistant = new CaptainAssistant
        {
            AccountId = accountId,
            Name = request.Name,
            Description = request.Description,
            Temperature = request.Temperature,
            ResponseGuidelines = request.ResponseGuidelines,
            Guardrails = request.Guardrails,
        };

        _dbContext.Set<CaptainAssistant>().Add(assistant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { accountId, id = assistant.Id }, assistant);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
        int accountId,
        int id,
        [FromBody] UpdateAssistantRequest request,
        CancellationToken cancellationToken)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .FirstOrDefaultAsync(a => a.Id == id && a.AccountId == accountId, cancellationToken);

        if (assistant is null) return NotFound();

        assistant.Name = request.Name;
        assistant.Description = request.Description;
        assistant.Temperature = request.Temperature;
        assistant.ResponseGuidelines = request.ResponseGuidelines;
        assistant.Guardrails = request.Guardrails;
        assistant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(
        int accountId,
        int id,
        CancellationToken cancellationToken)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .FirstOrDefaultAsync(a => a.Id == id && a.AccountId == accountId, cancellationToken);

        if (assistant is null) return NotFound();

        _dbContext.Set<CaptainAssistant>().Remove(assistant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/chat")]
    public async Task<ActionResult<AssistantChatResponse>> Chat(
        int accountId,
        int id,
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var context = new ConversationContext(
            request.ConversationId,
            request.PreviousMessages?.Select(m => new ChatMessage(m.Role, m.Content)).ToList() ?? [],
            request.ContactName,
            request.ContactEmail);

        var response = await _chatService.ChatAsync(id, request.Message, context, cancellationToken);

        return Ok(response);
    }

    public sealed record CreateAssistantRequest(
        string Name,
        string? Description = null,
        double Temperature = 0.7,
        string? ResponseGuidelines = null,
        string? Guardrails = null);

    public sealed record UpdateAssistantRequest(
        string Name,
        string? Description = null,
        double Temperature = 0.7,
        string? ResponseGuidelines = null,
        string? Guardrails = null);

    public sealed record ChatRequest(
        string Message,
        int ConversationId = 0,
        string? ContactName = null,
        string? ContactEmail = null,
        List<ChatMessageDto>? PreviousMessages = null);

    public sealed record ChatMessageDto(string Role, string Content);
}
