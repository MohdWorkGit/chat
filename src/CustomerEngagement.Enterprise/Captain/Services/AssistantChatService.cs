using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.Services;

public class AssistantChatService : IAssistantChatService
{
    private readonly DbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IToolRegistryService _toolRegistry;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssistantChatService> _logger;

    public AssistantChatService(
        DbContext dbContext,
        IEmbeddingService embeddingService,
        IToolRegistryService toolRegistry,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AssistantChatService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _toolRegistry = toolRegistry;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AssistantChatResponse> ChatAsync(
        int assistantId,
        string message,
        ConversationContext context,
        CancellationToken cancellationToken = default)
    {
        var assistant = await _dbContext.Set<CaptainAssistant>()
            .Include(a => a.Scenarios)
            .Include(a => a.CustomTools)
            .FirstOrDefaultAsync(a => a.Id == assistantId, cancellationToken)
            ?? throw new InvalidOperationException($"Assistant {assistantId} not found.");

        // Search for relevant document chunks via embeddings
        var relevantChunks = await _embeddingService.SearchSimilarAsync(message, topK: 5);

        // Build the system prompt
        var systemPrompt = BuildSystemPrompt(assistant, relevantChunks);

        // Build message history
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var prevMessage in context.PreviousMessages)
        {
            messages.Add(new { role = prevMessage.Role, content = prevMessage.Content });
        }

        messages.Add(new { role = "user", content = message });

        // Call Ollama API
        var ollamaBaseUrl = _configuration["Ollama:BaseUrl"] ?? "http://ollama:11434";
        var model = _configuration["Ollama:Model"] ?? "llama3";

        var requestBody = new
        {
            model,
            messages,
            stream = false,
            options = new { temperature = assistant.Temperature }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{ollamaBaseUrl}/api/chat",
                requestBody,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
            var content = result?.Message?.Content ?? "I'm sorry, I couldn't generate a response.";

            // Check if handoff is needed
            var requiresHandoff = content.Contains("[HANDOFF]", StringComparison.OrdinalIgnoreCase);
            var handoffReason = requiresHandoff
                ? content.Replace("[HANDOFF]", "").Trim()
                : null;

            if (requiresHandoff)
            {
                content = "Let me connect you with a human agent who can better assist you.";
            }

            return new AssistantChatResponse(content, requiresHandoff, handoffReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API for assistant {AssistantId}", assistantId);
            return new AssistantChatResponse(
                "I'm experiencing some difficulties right now. Let me connect you with a human agent.",
                RequiresHandoff: true,
                HandoffReason: "LLM service unavailable");
        }
    }

    private static string BuildSystemPrompt(CaptainAssistant assistant, IReadOnlyList<string> relevantChunks)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"You are {assistant.Name}, an AI assistant.");
        if (!string.IsNullOrWhiteSpace(assistant.Description))
        {
            sb.AppendLine(assistant.Description);
        }

        if (!string.IsNullOrWhiteSpace(assistant.ResponseGuidelines))
        {
            sb.AppendLine();
            sb.AppendLine("## Response Guidelines");
            sb.AppendLine(assistant.ResponseGuidelines);
        }

        if (!string.IsNullOrWhiteSpace(assistant.Guardrails))
        {
            sb.AppendLine();
            sb.AppendLine("## Guardrails");
            sb.AppendLine(assistant.Guardrails);
        }

        if (relevantChunks.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Relevant Knowledge Base Context");
            foreach (var chunk in relevantChunks)
            {
                sb.AppendLine($"---\n{chunk}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("If you cannot answer the question or the user requests a human agent, respond with [HANDOFF] followed by the reason.");

        return sb.ToString();
    }

    private sealed record OllamaResponse(OllamaMessage? Message);
    private sealed record OllamaMessage(string? Content);
}
