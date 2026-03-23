using CustomerEngagement.Enterprise.Captain.BackgroundJobs;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Controllers;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/captain/assistants/{assistantId:int}/documents")]
[Authorize]
public class CaptainDocumentsController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public CaptainDocumentsController(DbContext dbContext, IEmbeddingService embeddingService, IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptainDocument>>> GetAll(
        int accountId,
        int assistantId,
        CancellationToken cancellationToken)
    {
        var documents = await _dbContext.Set<CaptainDocument>()
            .Where(d => d.AssistantId == assistantId)
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(documents);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<CaptainDocument>> Upload(
        int accountId,
        int assistantId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var allowedTypes = new[] { "application/pdf", "text/plain", "text/markdown", "text/html" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            return BadRequest($"File type '{file.ContentType}' is not supported.");
        }

        // In production, upload to object storage (MinIO/S3)
        var fileUrl = $"/uploads/captain/{assistantId}/{Guid.NewGuid()}/{file.FileName}";

        var document = new CaptainDocument
        {
            AssistantId = assistantId,
            FileName = file.FileName,
            FileUrl = fileUrl,
            ContentType = file.ContentType,
        };

        _dbContext.Set<CaptainDocument>().Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _backgroundJobClient.Enqueue<ProcessDocumentJob>(j => j.ExecuteAsync(document.Id, CancellationToken.None));

        return CreatedAtAction(nameof(GetAll), new { accountId, assistantId }, document);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(
        int accountId,
        int assistantId,
        int id,
        CancellationToken cancellationToken)
    {
        var document = await _dbContext.Set<CaptainDocument>()
            .FirstOrDefaultAsync(d => d.Id == id && d.AssistantId == assistantId, cancellationToken);

        if (document is null) return NotFound();

        _dbContext.Set<CaptainDocument>().Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
