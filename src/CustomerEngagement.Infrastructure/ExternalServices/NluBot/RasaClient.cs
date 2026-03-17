using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.ExternalServices.NluBot;

public record RasaMessage(string Sender, string Message);

public record RasaResponse(
    [property: JsonPropertyName("recipient_id")] string RecipientId,
    string? Text,
    string? Image,
    RasaButton[]? Buttons,
    JsonElement? Custom);

public record RasaButton(string Title, string Payload);

public record RasaParseResponse(
    RasaIntent Intent,
    RasaEntity[] Entities,
    string? Text);

public record RasaIntent(string Name, double Confidence);

public record RasaEntity(
    string Entity,
    string Value,
    double Confidence,
    int Start,
    int End);

public class RasaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RasaClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RasaClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RasaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["Rasa:BaseUrl"] ?? "http://localhost:5005";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        var authToken = configuration["Rasa:AuthToken"];
        if (!string.IsNullOrEmpty(authToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
    }

    /// <summary>
    /// Send a message to the Rasa REST webhook and get bot responses.
    /// </summary>
    public async Task<IReadOnlyList<RasaResponse>> SendMessageAsync(
        string senderId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var request = new RasaMessage(senderId, message);

        _logger.LogDebug("Sending message to Rasa for sender {SenderId}: {Message}", senderId, message);

        var response = await _httpClient.PostAsJsonAsync(
            "/webhooks/rest/webhook",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responses = await response.Content.ReadFromJsonAsync<RasaResponse[]>(JsonOptions, cancellationToken);

        _logger.LogDebug("Received {Count} responses from Rasa for sender {SenderId}",
            responses?.Length ?? 0, senderId);

        return responses ?? Array.Empty<RasaResponse>();
    }

    /// <summary>
    /// Parse a message using the Rasa NLU model to extract intent and entities.
    /// </summary>
    public async Task<RasaParseResponse> ParseMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var request = new { text = message };

        _logger.LogDebug("Parsing message with Rasa NLU: {Message}", message);

        var response = await _httpClient.PostAsJsonAsync(
            "/model/parse",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RasaParseResponse>(JsonOptions, cancellationToken);

        _logger.LogDebug("Rasa NLU parsed intent: {Intent} (confidence: {Confidence})",
            result?.Intent.Name, result?.Intent.Confidence);

        return result ?? throw new InvalidOperationException("Empty response from Rasa NLU parse.");
    }

    /// <summary>
    /// Check if the Rasa server is available and a model is loaded.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/status", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rasa health check failed");
            return false;
        }
    }

    /// <summary>
    /// Trigger an intent directly for a conversation.
    /// </summary>
    public async Task<IReadOnlyList<RasaResponse>> TriggerIntentAsync(
        string senderId,
        string intentName,
        Dictionary<string, object>? entities = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            name = intentName,
            entities = entities ?? new Dictionary<string, object>()
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/conversations/{senderId}/trigger_intent",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RasaIntentTriggerResponse>(JsonOptions, cancellationToken);

        return result?.Messages ?? Array.Empty<RasaResponse>();
    }
}

internal record RasaIntentTriggerResponse(RasaResponse[] Messages);
