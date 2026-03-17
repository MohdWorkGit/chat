namespace CustomerEngagement.Enterprise.Captain.Services;

public record CopilotSuggestion(string Content, double Confidence);

public interface ICopilotService
{
    Task<CopilotSuggestion> SuggestReplyAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<string> RewriteAsync(
        string text,
        string tone,
        CancellationToken cancellationToken = default);

    Task<string> SummarizeAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SuggestLabelsAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SuggestFollowUpAsync(
        int conversationId,
        CancellationToken cancellationToken = default);
}
