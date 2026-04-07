using CustomerEngagement.Core.Entities;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/assistants/{assistantId:int}/inboxes")]
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
        int assistantId,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var connections = await _dbContext.Set<CaptainInbox>()
            .Where(ci => ci.AssistantId == assistantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(connections);
    }

    [HttpPost]
    public async Task<ActionResult<CaptainInbox>> Connect(
        int accountId,
        int assistantId,
        [FromBody] ConnectInboxRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var inboxBelongs = await _dbContext.Set<Inbox>()
            .AsNoTracking()
            .AnyAsync(i => i.Id == request.InboxId && i.AccountId == accountId, cancellationToken);

        if (!inboxBelongs)
        {
            return NotFound(new { error = "Inbox not found in this account." });
        }

        var existing = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(
                ci => ci.AssistantId == assistantId && ci.InboxId == request.InboxId,
                cancellationToken);

        if (existing is not null)
        {
            existing.Active = request.Active;
            existing.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(existing);
        }

        var connection = new CaptainInbox
        {
            AssistantId = assistantId,
            InboxId = request.InboxId,
            Active = request.Active,
        };

        _dbContext.Set<CaptainInbox>().Add(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { accountId, assistantId },
            connection);
    }

    [HttpPut("{inboxId:int}")]
    public async Task<ActionResult> UpdateStatus(
        int accountId,
        int assistantId,
        int inboxId,
        [FromBody] UpdateInboxStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var connection = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(
                ci => ci.AssistantId == assistantId && ci.InboxId == inboxId,
                cancellationToken);

        if (connection is null) return NotFound();

        connection.Active = request.Active;
        connection.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{inboxId:int}")]
    public async Task<ActionResult> Disconnect(
        int accountId,
        int assistantId,
        int inboxId,
        CancellationToken cancellationToken)
    {
        if (!await AssistantBelongsToAccountAsync(accountId, assistantId, cancellationToken))
        {
            return NotFound();
        }

        var connection = await _dbContext.Set<CaptainInbox>()
            .FirstOrDefaultAsync(
                ci => ci.AssistantId == assistantId && ci.InboxId == inboxId,
                cancellationToken);

        if (connection is null) return NotFound();

        _dbContext.Set<CaptainInbox>().Remove(connection);
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

    public sealed record ConnectInboxRequest(int InboxId, bool Active = true);

    public sealed record UpdateInboxStatusRequest(bool Active);
}
