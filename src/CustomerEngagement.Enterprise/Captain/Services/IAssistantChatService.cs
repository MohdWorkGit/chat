namespace CustomerEngagement.Enterprise.Captain.Services;

public record ConversationContext(
    int ConversationId,
    int ContactId,
    IReadOnlyList<ChatMessage> PreviousMessages
);

public record ChatMessage(string Role, string Content);

public record AssistantChatResponse(
    string Content,
    bool RequiresHandoff,
    string? HandoffReason
);

public interface IAssistantChatService
{
    Task<AssistantChatResponse> ChatAsync(
        int assistantId,
        string message,
        ConversationContext context,
        CancellationToken cancellationToken = default);
}
