using System.Net.Http.Json;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace CustomerEngagement.Enterprise.Captain.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly DbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        DbContext dbContext,
        HttpClient httpClient,
        ILogger<EmbeddingService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                model = "nomic-embed-text",
                prompt = text
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://ollama:11434/api/embeddings",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
                cancellationToken: cancellationToken);

            return result?.Embedding ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding from Ollama");
            return [];
        }
    }

    public async Task<IReadOnlyList<ArticleEmbedding>> SearchSimilarAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(query, cancellationToken);

        if (queryEmbedding.Length == 0)
        {
            return [];
        }

        var queryVector = new Vector(queryEmbedding);

        var results = await _dbContext.Set<ArticleEmbedding>()
            .OrderBy(e => e.Embedding.L2Distance(queryVector))
            .Take(topK)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task StoreEmbeddingAsync(
        int articleId,
        string chunkText,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        var entity = new ArticleEmbedding
        {
            ArticleId = articleId,
            ChunkText = chunkText,
            Embedding = new Vector(embedding),
        };

        _dbContext.Set<ArticleEmbedding>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record OllamaEmbeddingResponse(float[]? Embedding);
}
