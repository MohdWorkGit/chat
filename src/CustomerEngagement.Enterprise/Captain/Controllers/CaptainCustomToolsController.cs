using CustomerEngagement.Enterprise.Captain.DTOs;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/assistants/{assistantId:int}/tools")]
[Authorize]
public class CaptainCustomToolsController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly IToolRegistryService _toolRegistry;

    public CaptainCustomToolsController(DbContext dbContext, IToolRegistryService toolRegistry)
    {
        _dbContext = dbContext;
        _toolRegistry = toolRegistry;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptainCustomTool>>> GetAll(
        int accountId,
        int assistantId,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var tools = await _toolRegistry.GetToolsForAssistantAsync(assistantId, cancellationToken);
        return Ok(tools);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CaptainCustomTool>> GetById(
        int accountId,
        int assistantId,
        int id,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var tool = await _dbContext.Set<CaptainCustomTool>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.AssistantId == assistantId, cancellationToken);

        if (tool is null) return NotFound();

        return Ok(tool);
    }

    [HttpPost]
    public async Task<ActionResult<CaptainCustomTool>> Create(
        int accountId,
        int assistantId,
        [FromBody] CreateToolRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var tool = new CaptainCustomTool
        {
            AssistantId = assistantId,
            Name = request.Name,
            Description = request.Description,
            Parameters = request.Parameters ?? "{}",
            EndpointUrl = request.EndpointUrl,
        };

        await _toolRegistry.RegisterToolAsync(tool, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { accountId, assistantId, id = tool.Id }, tool);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
        int accountId,
        int assistantId,
        int id,
        [FromBody] UpdateToolRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var tool = await _dbContext.Set<CaptainCustomTool>()
            .FirstOrDefaultAsync(t => t.Id == id && t.AssistantId == assistantId, cancellationToken);

        if (tool is null) return NotFound();

        tool.Name = request.Name;
        tool.Description = request.Description;
        tool.EndpointUrl = request.EndpointUrl;
        if (request.Parameters is not null)
        {
            tool.Parameters = request.Parameters;
        }
        tool.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(
        int accountId,
        int assistantId,
        int id,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var exists = await _dbContext.Set<CaptainCustomTool>()
            .AnyAsync(t => t.Id == id && t.AssistantId == assistantId, cancellationToken);

        if (!exists) return NotFound();

        await _toolRegistry.UnregisterToolAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/execute")]
    public async Task<ActionResult<ToolExecutionResult>> Execute(
        int accountId,
        int assistantId,
        int id,
        [FromBody] Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var exists = await _dbContext.Set<CaptainCustomTool>()
            .AsNoTracking()
            .AnyAsync(t => t.Id == id && t.AssistantId == assistantId, cancellationToken);

        if (!exists) return NotFound();

        var result = await _toolRegistry.ExecuteToolAsync(id, parameters, cancellationToken);
        return Ok(result);
    }

    private Task<bool> AssistantBelongsToAccountAsync(
        int accountId,
        int assistantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<CaptainAssistant>()
            .AsNoTracking()
            .AnyAsync(a => a.Id == assistantId && a.AccountId == accountId, cancellationToken);
    }

    public sealed record CreateToolRequest(
        string Name,
        string EndpointUrl,
        string? Description = null,
        string? Parameters = null);

    public sealed record UpdateToolRequest(
        string Name,
        string EndpointUrl,
        string? Description = null,
        string? Parameters = null);
}
