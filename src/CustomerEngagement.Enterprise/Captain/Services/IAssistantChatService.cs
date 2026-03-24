using CustomerEngagement.Enterprise.Captain.DTOs;

namespace CustomerEngagement.Enterprise.Captain.Services;

public interface IAssistantChatService
{
    Task<AssistantChatResponse> ChatAsync(
        int assistantId,
        string message,
        ConversationContext conversationContext,
        CancellationToken cancellationToken = default);
}
