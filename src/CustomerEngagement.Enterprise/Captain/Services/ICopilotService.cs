namespace CustomerEngagement.Enterprise.Captain.Services;

public record CopilotSuggestion(string Content, double Confidence);
public record RewriteResult(string OriginalText, string RewrittenText, string Tone);
public record ConversationSummary(string Summary, IReadOnlyList<string> KeyPoints);

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
