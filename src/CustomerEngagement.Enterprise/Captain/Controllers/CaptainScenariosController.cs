using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/assistants/{assistantId:int}/scenarios")]
[Authorize]
public class CaptainScenariosController : ControllerBase
{
    private readonly DbContext _dbContext;

    public CaptainScenariosController(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptainScenario>>> GetAll(
        int accountId,
        int assistantId,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var scenarios = await _dbContext.Set<CaptainScenario>()
            .Where(s => s.AssistantId == assistantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(scenarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CaptainScenario>> GetById(
        int accountId,
        int assistantId,
        int id,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var scenario = await _dbContext.Set<CaptainScenario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.AssistantId == assistantId, cancellationToken);

        if (scenario is null) return NotFound();

        return Ok(scenario);
    }

    [HttpPost]
    public async Task<ActionResult<CaptainScenario>> Create(
        int accountId,
        int assistantId,
        [FromBody] CreateScenarioRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var scenario = new CaptainScenario
        {
            AssistantId = assistantId,
            Title = request.Title,
            Description = request.Description,
            Steps = request.Steps ?? "[]",
        };

        _dbContext.Set<CaptainScenario>().Add(scenario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { accountId, assistantId, id = scenario.Id }, scenario);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
        int accountId,
        int assistantId,
        int id,
        [FromBody] UpdateScenarioRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var scenario = await _dbContext.Set<CaptainScenario>()
            .FirstOrDefaultAsync(s => s.Id == id && s.AssistantId == assistantId, cancellationToken);

        if (scenario is null) return NotFound();

        scenario.Title = request.Title;
        scenario.Description = request.Description;
        if (request.Steps is not null)
        {
            scenario.Steps = request.Steps;
        }
        scenario.UpdatedAt = DateTime.UtcNow;

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

        var scenario = await _dbContext.Set<CaptainScenario>()
            .FirstOrDefaultAsync(s => s.Id == id && s.AssistantId == assistantId, cancellationToken);

        if (scenario is null) return NotFound();

        _dbContext.Set<CaptainScenario>().Remove(scenario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
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

    public sealed record CreateScenarioRequest(
        string Title,
        string? Description = null,
        string? Steps = null);

    public sealed record UpdateScenarioRequest(
        string Title,
        string? Description = null,
        string? Steps = null);
}
