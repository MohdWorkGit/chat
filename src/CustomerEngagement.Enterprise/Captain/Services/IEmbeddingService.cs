using CustomerEngagement.Enterprise.Captain.Entities;

namespace CustomerEngagement.Enterprise.Captain.Services;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArticleEmbedding>> SearchSimilarAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default);

    Task StoreEmbeddingAsync(
        int articleId,
        string chunkText,
        float[] embedding,
        CancellationToken cancellationToken = default);
}
