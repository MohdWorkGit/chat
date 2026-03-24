using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Enterprise.Captain.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.Services;

public class CopilotService : ICopilotService
{
    private readonly DbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CopilotService> _logger;

    public CopilotService(
        DbContext dbContext,
        HttpClient httpClient,
        ILogger<CopilotService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CopilotSuggestion> SuggestReplyAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var messages = await GetConversationMessagesAsync(conversationId, cancellationToken);
        var prompt = BuildSuggestReplyPrompt(messages);
        var response = await CallOllamaAsync(prompt, cancellationToken);

        return new CopilotSuggestion(response, 0.85);
    }

    public async Task<RewriteResult> RewriteAsync(
        string text,
        string tone,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"Rewrite the following customer support message in a {tone} tone. " +
                     $"Only return the rewritten text, no explanations.\n\nOriginal: {text}";

        var response = await CallOllamaAsync(prompt, cancellationToken);

        return new RewriteResult(text, response, tone);
    }

    public async Task<ConversationSummary> SummarizeAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var messages = await GetConversationMessagesAsync(conversationId, cancellationToken);
        var prompt = "Summarize the following customer support conversation. " +
                     "Provide a brief summary and key points as a JSON object with 'summary' (string) and 'keyPoints' (array of strings).\n\n" +
                     FormatMessages(messages);

        var response = await CallOllamaAsync(prompt, cancellationToken);

        try
        {
            var parsed = JsonSerializer.Deserialize<SummaryJson>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return new ConversationSummary(
                parsed?.Summary ?? response,
                parsed?.KeyPoints ?? []);
        }
        catch
        {
            return new ConversationSummary(response, []);
        }
    }

    public async Task<IReadOnlyList<string>> SuggestLabelsAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var messages = await GetConversationMessagesAsync(conversationId, cancellationToken);
        var prompt = "Based on the following customer support conversation, suggest relevant labels/tags. " +
                     "Return only a JSON array of strings.\n\n" +
                     FormatMessages(messages);

        var response = await CallOllamaAsync(prompt, cancellationToken);

        try
        {
            return JsonSerializer.Deserialize<List<string>>(response) ?? [];
        }
        catch
        {
            return [response.Trim()];
        }
    }

    public async Task<IReadOnlyList<string>> SuggestFollowUpAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var messages = await GetConversationMessagesAsync(conversationId, cancellationToken);
        var prompt = "Based on the following customer support conversation, suggest follow-up questions " +
                     "the agent could ask. Return only a JSON array of strings with 2-3 suggestions.\n\n" +
                     FormatMessages(messages);

        var response = await CallOllamaAsync(prompt, cancellationToken);

        try
        {
            return JsonSerializer.Deserialize<List<string>>(response) ?? [];
        }
        catch
        {
            return [response.Trim()];
        }
    }

    private async Task<List<Message>> GetConversationMessagesAsync(
        int conversationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Message>()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static string BuildSuggestReplyPrompt(List<Message> messages)
    {
        return "You are a customer support copilot. Based on the conversation below, " +
               "suggest a helpful reply for the support agent to send. " +
               "Only return the suggested reply text, no explanations.\n\n" +
               FormatMessages(messages);
    }

    private static string FormatMessages(List<Message> messages)
    {
        return string.Join("\n", messages.Select(m =>
            $"[{(m.SenderType == "User" ? "Customer" : "Agent")}]: {m.Content}"));
    }

    private async Task<string> CallOllamaAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var request = new
            {
                model = "llama3",
                prompt,
                stream = false,
                options = new { temperature = 0.3 }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://ollama:11434/api/generate",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken);

            return result?.Response ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for copilot");
            return "Unable to generate suggestion at this time.";
        }
    }

    private sealed record OllamaGenerateResponse(string? Response);
    private sealed record SummaryJson(string? Summary, List<string>? KeyPoints);
}
