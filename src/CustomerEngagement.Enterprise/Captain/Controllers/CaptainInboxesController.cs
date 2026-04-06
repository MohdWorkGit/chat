using CustomerEngagement.Core.Entities;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/inboxes")]
[Authorize]
public class CaptainInboxesController : ControllerBase
{
    private readonly DbContext _dbContext;

    public CaptainInboxesController(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptainInbox>>> GetAll(
        int accountId,
        CancellationToken cancellationToken)
    {
        var connections = await _dbContext.Set<CaptainInbox>()
            .AsNoTracking()
            .Where(ci => _dbContext.Set<CaptainAssistant>()
                .Any(a => a.Id == ci.AssistantId && a.AccountId == accountId))
            .ToListAsync(cancellationToken);

        return Ok(connections);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CaptainInbox>> GetById(
        int accountId,
        int id,
        CancellationToken cancellationToken)
    {
        var connection = await _dbContext.Set<CaptainInbox>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (connection is null) return NotFound();

        var ownsAssistant = await _dbContext.Set<CaptainAssistant>()
            .AnyAsync(a => a.Id == connection.AssistantId && a.AccountId == accountId, cancellationToken);

        if (!ownsAssistant) return NotFound();

        return Ok(connection);
    }

    [HttpPost]
    public async Task<ActionResult<CaptainInbox>> Connect(
        int accountId,
        [FromBody] ConnectCaptainInboxRequest request,
        CancellationToken cancellationToken)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .FirstOrDefaultAsync(a => a.Id == request.AssistantId && a.AccountId == accountId, cancellationToken);

        if (assistant is null)
            return NotFound(new { error = "Assistant not found in this account." });

        var inbox = await _dbContext.Set<Inbox>()
            .FirstOrDefaultAsync(i => i.Id == request.InboxId && i.AccountId == accountId, cancellationToken);

        if (inbox is null)
            return NotFound(new { error = "Inbox not found in this account." });

        var existing = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(
                ci => ci.AssistantId == request.AssistantId && ci.InboxId == request.InboxId,
                cancellationToken);

        if (existing is not null)
            return Conflict(new { error = "This assistant is already connected to this inbox.", id = existing.Id });

        var connection = new CaptainInbox
        {
            AssistantId = request.AssistantId,
            InboxId = request.InboxId,
            Active = request.Active,
        };

        _dbContext.Set<CaptainInbox>().Add(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { accountId, id = connection.Id }, connection);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> SetActive(
        int accountId,
        int id,
        [FromBody] UpdateCaptainInboxRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (connection is null) return NotFound();

        var ownsAssistant = await _dbContext.Set<CaptainAssistant>()
            .AnyAsync(a => a.Id == connection.AssistantId && a.AccountId == accountId, cancellationToken);

        if (!ownsAssistant) return NotFound();

        connection.Active = request.Active;
        connection.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Disconnect(
        int accountId,
        int id,
        CancellationToken cancellationToken)
    {
        var connection = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (connection is null) return NotFound();

        var ownsAssistant = await _dbContext.Set<CaptainAssistant>()
            .AnyAsync(a => a.Id == connection.AssistantId && a.AccountId == accountId, cancellationToken);

        if (!ownsAssistant) return NotFound();

        _dbContext.Set<CaptainInbox>().Remove(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record ConnectCaptainInboxRequest(int AssistantId, int InboxId, bool Active = true);

    public sealed record UpdateCaptainInboxRequest(bool Active);
}
