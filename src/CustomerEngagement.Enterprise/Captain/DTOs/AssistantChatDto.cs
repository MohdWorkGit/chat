namespace CustomerEngagement.Enterprise.Captain.DTOs;

public record ConversationContext(
    int ConversationId,
    IReadOnlyList<ChatMessage> PreviousMessages,
    string? ContactName = null,
    string? ContactEmail = null
);

public record ChatMessage(string Role, string Content);

public record AssistantChatResponse(
    string Content,
    IReadOnlyList<string>? SourceDocuments = null,
    bool HandoffRequested = false
);
