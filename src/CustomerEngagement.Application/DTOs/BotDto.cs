namespace CustomerEngagement.Application.DTOs;

public class BotMessageRequest
{
    public int AccountId { get; set; }
    public long ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SenderIdentifier { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class BotResponse
{
    public bool Success { get; set; }
    public List<BotResponseMessage> Messages { get; set; } = new();
    public string? Intent { get; set; }
    public double? Confidence { get; set; }
    public bool HandoffToAgent { get; set; }
}

public class BotResponseMessage
{
    public string Text { get; set; } = string.Empty;
    public string? ContentType { get; set; } = "text";
    public List<BotButton>? Buttons { get; set; }
}

public class BotButton
{
    public string Title { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
