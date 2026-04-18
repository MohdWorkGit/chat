using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.ExternalServices.Llm;

public record ChatMessage(string Role, string Content);

public record ChatCompletionRequest(
    string Model,
    IReadOnlyList<ChatMessage> Messages,
    double? Temperature = null,
    int? MaxTokens = null,
    bool Stream = false);

public record ChatCompletionResponse(
    string Model,
    ChatMessage Message,
    bool Done,
    long? TotalDuration = null,
    int? PromptEvalCount = null,
    int? EvalCount = null);

public record EmbeddingRequest(string Model, string Prompt);

public record EmbeddingResponse(float[] Embedding);

public class OllamaClient : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaClient> _logger;
    private readonly string _defaultModel;
    private readonly string _defaultEmbeddingModel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OllamaClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        _defaultModel = configuration["Ollama:DefaultModel"] ?? "llama3";
        _defaultEmbeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
    }

    public async Task<ChatCompletionResponse> ChatCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        string? model = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequest(
            Model: model ?? _defaultModel,
            Messages: messages,
            Temperature: temperature,
            MaxTokens: maxTokens,
            Stream: false);

        _logger.LogDebug("Sending chat completion request to Ollama with model {Model}", request.Model);

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken);

        _logger.LogDebug("Received chat completion response from Ollama. Eval count: {EvalCount}", result?.EvalCount);

        return result ?? throw new InvalidOperationException("Empty response from Ollama chat API.");
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        var request = new EmbeddingRequest(
            Model: model ?? _defaultEmbeddingModel,
            Prompt: text);

        _logger.LogDebug("Generating embedding with model {Model}", request.Model);

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, cancellationToken);

        return result?.Embedding ?? throw new InvalidOperationException("Empty response from Ollama embeddings API.");
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsBatchAsync(
        IReadOnlyList<string> texts,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<float[]>();

        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text, model, cancellationToken);
            results.Add(embedding);
        }

        return results;
    }

    async Task<LlmCompletionResult> ILlmService.GenerateCompletionAsync(
        IEnumerable<LlmChatMessage> messages,
        string? model,
        float temperature,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var mapped = messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList();
        var response = await ChatCompletionAsync(mapped, model, temperature, maxTokens, cancellationToken);
        var tokens = (response.PromptEvalCount ?? 0) + (response.EvalCount ?? 0);
        return new LlmCompletionResult(response.Message.Content, tokens);
    }

    async Task<LlmEmbeddingResult> ILlmService.GenerateEmbeddingAsync(
        string text,
        string? model,
        CancellationToken cancellationToken)
    {
        var embedding = await GenerateEmbeddingAsync(text, model, cancellationToken);
        return new LlmEmbeddingResult(embedding, 0);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama health check failed");
            return false;
        }
    }

    /// <summary>
    /// Chat completion using OpenAI-compatible endpoint (/v1/chat/completions).
    /// </summary>
    public async Task<OpenAiChatResponse> OpenAiChatCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        string? model = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = model ?? _defaultModel,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature,
            max_tokens = maxTokens,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty response from Ollama OpenAI-compatible API.");
    }
}

public record OpenAiChatResponse(
    string Id,
    string Object,
    long Created,
    string Model,
    IReadOnlyList<OpenAiChoice> Choices,
    OpenAiUsage? Usage);

public record OpenAiChoice(int Index, OpenAiMessage Message, string? FinishReason);

public record OpenAiMessage(string Role, string Content);

public record OpenAiUsage(int PromptTokens, int CompletionTokens, int TotalTokens);
