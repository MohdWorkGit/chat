using CustomerEngagement.Enterprise.Captain.DTOs;

namespace CustomerEngagement.Enterprise.Captain.Services;

public interface ICopilotService
{
    Task<CopilotSuggestion> SuggestReplyAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<RewriteResult> RewriteAsync(
        string text,
        string tone,
        CancellationToken cancellationToken = default);

    Task<ConversationSummary> SummarizeAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SuggestLabelsAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SuggestFollowUpAsync(
        int conversationId,
        CancellationToken cancellationToken = default);
}
