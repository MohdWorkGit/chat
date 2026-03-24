using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CustomerEngagement.Enterprise.Captain.DTOs;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.Services;

public class AssistantChatService : IAssistantChatService
{
    private readonly DbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IToolRegistryService _toolRegistryService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AssistantChatService> _logger;

    public AssistantChatService(
        DbContext dbContext,
        IEmbeddingService embeddingService,
        IToolRegistryService toolRegistryService,
        HttpClient httpClient,
        ILogger<AssistantChatService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _toolRegistryService = toolRegistryService;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AssistantChatResponse> ChatAsync(
        int assistantId,
        string message,
        ConversationContext conversationContext,
        CancellationToken cancellationToken = default)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assistantId, cancellationToken)
            ?? throw new InvalidOperationException($"Assistant with id {assistantId} not found.");

        // Retrieve relevant documents via embedding similarity search
        var relevantChunks = await _embeddingService.SearchSimilarAsync(message, topK: 5);

        // Build system prompt with assistant configuration
        var systemPrompt = BuildSystemPrompt(assistant, relevantChunks);

        // Build conversation messages for the LLM
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var prevMsg in conversationContext.PreviousMessages)
        {
            messages.Add(new { role = prevMsg.Role, content = prevMsg.Content });
        }

        messages.Add(new { role = "user", content = message });

        // Call Ollama API for chat completion
        var ollamaRequest = new
        {
            model = "llama3",
            messages,
            stream = false,
            options = new
            {
                temperature = assistant.Temperature,
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "http://ollama:11434/api/chat",
                ollamaRequest,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);

            var content = result?.Message?.Content ?? "I'm sorry, I couldn't generate a response.";

            // Check for handoff signals in the response
            var handoffRequested = content.Contains("[HANDOFF]", StringComparison.OrdinalIgnoreCase);
            if (handoffRequested)
            {
                content = content.Replace("[HANDOFF]", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            var sourceDocuments = relevantChunks
                .Select(c => c.ChunkText[..Math.Min(100, c.ChunkText.Length)])
                .ToList();

            return new AssistantChatResponse(content, sourceDocuments, handoffRequested);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API for assistant {AssistantId}", assistantId);
            return new AssistantChatResponse(
                "I'm sorry, I'm having trouble processing your request. Let me connect you with a human agent.",
                HandoffRequested: true);
        }
    }

    private static string BuildSystemPrompt(CaptainAssistant assistant, IReadOnlyList<ArticleEmbedding> relevantChunks)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"You are {assistant.Name}, a customer support AI assistant.");

        if (!string.IsNullOrWhiteSpace(assistant.Description))
        {
            sb.AppendLine(assistant.Description);
        }

        if (!string.IsNullOrWhiteSpace(assistant.ResponseGuidelines))
        {
            sb.AppendLine("\n## Response Guidelines");
            sb.AppendLine(assistant.ResponseGuidelines);
        }

        if (!string.IsNullOrWhiteSpace(assistant.Guardrails))
        {
            sb.AppendLine("\n## Guardrails");
            sb.AppendLine(assistant.Guardrails);
        }

        if (relevantChunks.Count > 0)
        {
            sb.AppendLine("\n## Relevant Knowledge Base Content");
            foreach (var chunk in relevantChunks)
            {
                sb.AppendLine($"---\n{chunk.ChunkText}\n---");
            }
        }

        sb.AppendLine("\nIf you cannot answer the question or the customer requests a human agent, respond with [HANDOFF] in your message.");

        return sb.ToString();
    }

    private sealed record OllamaResponse(OllamaMessage? Message);
    private sealed record OllamaMessage(string? Content);
}
