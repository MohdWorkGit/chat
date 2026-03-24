namespace CustomerEngagement.Enterprise.Captain.DTOs;

public record CopilotSuggestion(string Content, double Confidence);
public record RewriteResult(string OriginalText, string RewrittenText, string Tone);
public record ConversationSummary(string Summary, IReadOnlyList<string> KeyPoints);
