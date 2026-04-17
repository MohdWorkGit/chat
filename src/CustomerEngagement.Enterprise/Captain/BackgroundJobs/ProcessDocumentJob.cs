using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.BackgroundJobs;

public class ProcessDocumentJob
{
    private const int ChunkSize = 500;

    private readonly DbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IStorageService _storageService;
    private readonly ILogger<ProcessDocumentJob> _logger;

    public ProcessDocumentJob(
        DbContext dbContext,
        IEmbeddingService embeddingService,
        IStorageService storageService,
        ILogger<ProcessDocumentJob> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a Captain document by reading its content, splitting into chunks,
    /// generating embeddings, and storing them. Intended to be enqueued by Hangfire.
    /// </summary>
    public async Task ExecuteAsync(int documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document processing job for document {DocumentId}", documentId);

        var document = await _dbContext.Set<CaptainDocument>()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document is null)
        {
            _logger.LogWarning("CaptainDocument {DocumentId} not found", documentId);
            return;
        }

        try
        {
            string content;
            await using (var stream = await _storageService.DownloadFileAsync(document.FileUrl, cancellationToken))
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Document {DocumentId} has no content", documentId);
                return;
            }

            var chunks = SplitIntoChunks(content, ChunkSize);

            _logger.LogInformation(
                "Document {DocumentId} split into {ChunkCount} chunks",
                documentId, chunks.Count);

            for (var i = 0; i < chunks.Count; i++)
            {
                try
                {
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i], cancellationToken);
                    await _embeddingService.StoreEmbeddingAsync(document.Id, chunks[i], embedding, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to process chunk {ChunkIndex}/{ChunkCount} for document {DocumentId}",
                        i + 1, chunks.Count, documentId);
                }
            }

            document.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document processing completed for document {DocumentId}. Processed {ChunkCount} chunks",
                documentId, chunks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document processing failed for document {DocumentId}", documentId);
        }
    }

    private static List<string> SplitIntoChunks(string content, int chunkSize)
    {
        var chunks = new List<string>();

        for (var i = 0; i < content.Length; i += chunkSize)
        {
            var length = Math.Min(chunkSize, content.Length - i);
            var chunk = content.Substring(i, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }
}
