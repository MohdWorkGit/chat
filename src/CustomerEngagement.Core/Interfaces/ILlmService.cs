namespace CustomerEngagement.Core.Interfaces;

public record LlmChatMessage(string Role, string Content);
public record LlmCompletionResult(string Content, int TokensUsed);
public record LlmEmbeddingResult(float[] Embedding, int TokensUsed);

public interface ILlmService
{
    Task<LlmCompletionResult> GenerateCompletionAsync(
        IEnumerable<LlmChatMessage> messages,
        string? model = null,
        float temperature = 0.7f,
        int maxTokens = 1024,
        CancellationToken cancellationToken = default);

    Task<LlmEmbeddingResult> GenerateEmbeddingAsync(
        string text,
        string? model = null,
        CancellationToken cancellationToken = default);
}
