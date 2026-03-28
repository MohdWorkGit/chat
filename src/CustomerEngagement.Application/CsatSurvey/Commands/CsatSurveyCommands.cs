using MediatR;

namespace CustomerEngagement.Application.CsatSurvey.Commands;

public record SubmitCsatResponseCommand(
    int AccountId,
    int ConversationId,
    int MessageId,
    int ContactId,
    int? AssigneeId,
    int Rating,
    string? FeedbackText) : IRequest<CsatResponseResult>;

public record UpdateCsatResponseCommand(
    int Id,
    int? Rating,
    string? FeedbackText) : IRequest<CsatResponseResult>;

public class CsatResponseResult
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int? Rating { get; set; }
    public string? FeedbackText { get; set; }
    public DateTime CreatedAt { get; set; }
}
