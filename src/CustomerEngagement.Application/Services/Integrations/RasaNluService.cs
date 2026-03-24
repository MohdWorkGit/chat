using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerEngagement.Application.Services.Integrations;

public class RasaNluServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5005";
    public int TimeoutSeconds { get; set; } = 30;
}

public class RasaNluService : IRasaNluService
{
    private readonly HttpClient _httpClient;
    private readonly RasaNluServiceOptions _options;
    private readonly ILogger<RasaNluService> _logger;

    public RasaNluService(
        HttpClient httpClient,
        IOptions<RasaNluServiceOptions> options,
        ILogger<RasaNluService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<BotResponse> ProcessMessageAsync(BotMessageRequest message, CancellationToken cancellationToken = default)
    {
        try
        {
            var rasaRequest = new
            {
                sender = message.SenderIdentifier ?? $"conversation_{message.ConversationId}",
                message = message.Message,
                metadata = message.Metadata
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/webhooks/rest/webhook",
                rasaRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Rasa NLU returned non-success status {StatusCode} for conversation {ConversationId}",
                    response.StatusCode, message.ConversationId);

                return new BotResponse { Success = false };
            }

            var rasaResponses = await response.Content.ReadFromJsonAsync<List<RasaResponseMessage>>(cancellationToken: cancellationToken);

            var botResponse = new BotResponse
            {
                Success = true,
                Messages = rasaResponses?.Select(r => new BotResponseMessage
                {
                    Text = r.Text ?? string.Empty,
                    ContentType = "text",
                    Buttons = r.Buttons?.Select(b => new BotButton
                    {
                        Title = b.Title ?? string.Empty,
                        Payload = b.Payload ?? string.Empty
                    }).ToList()
                }).ToList() ?? new()
            };

            // Check if any response indicates a handoff to a human agent
            if (rasaResponses?.Any(r => r.Custom?.ContainsKey("handoff") == true) == true)
            {
                botResponse.HandoffToAgent = true;
            }

            _logger.LogInformation(
                "Processed message through Rasa NLU for conversation {ConversationId}, got {MessageCount} responses",
                message.ConversationId, botResponse.Messages.Count);

            return botResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Rasa NLU service for conversation {ConversationId}",
                message.ConversationId);

            return new BotResponse
            {
                Success = false,
                Messages = new List<BotResponseMessage>
                {
                    new() { Text = "I'm having trouble processing your request. Let me connect you with an agent." }
                },
                HandoffToAgent = true
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Rasa NLU request timed out for conversation {ConversationId}",
                message.ConversationId);

            return new BotResponse { Success = false, HandoffToAgent = true };
        }
    }

    private class RasaResponseMessage
    {
        public string? RecipientId { get; set; }
        public string? Text { get; set; }
        public string? Image { get; set; }
        public List<RasaButton>? Buttons { get; set; }
        public Dictionary<string, object>? Custom { get; set; }
    }

    private class RasaButton
    {
        public string? Title { get; set; }
        public string? Payload { get; set; }
    }
}
