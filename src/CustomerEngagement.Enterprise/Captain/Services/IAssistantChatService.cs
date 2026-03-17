namespace CustomerEngagement.Enterprise.Captain.Services;

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

public interface IAssistantChatService
{
    Task<AssistantChatResponse> ChatAsync(
        int assistantId,
        string message,
        ConversationContext conversationContext,
        CancellationToken cancellationToken = default);
}
