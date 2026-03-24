using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Application.DTOs;

public record ConversationDto(
    int Id, int AccountId, int InboxId, int ContactId,
    int? AssigneeId, int? TeamId, int DisplayId,
    string Status, string Priority, string? Identifier,
    DateTime? SnoozedUntil, bool Muted,
    DateTime? LastActivityAt, DateTime CreatedAt, DateTime UpdatedAt,
    ContactSummaryDto? Contact, UserSummaryDto? Assignee,
    InboxSummaryDto? Inbox, int MessageCount, IReadOnlyList<string> Labels);

public record ConversationListDto(
    IReadOnlyList<ConversationDto> Data, MetaDto Meta);

public record UpdateConversationRequest(
    string? Status, int? AssigneeId, int? TeamId, string? Priority, bool? Muted);

public class ConversationFilterDto
{
    public ConversationStatus? Status { get; set; }
    public int? AssigneeId { get; set; }
    public int? InboxId { get; set; }
    public int? TeamId { get; set; }
    public string? LabelName { get; set; }
    public string? Query { get; set; }
    public string SortBy { get; set; } = "created_at";
    public bool SortDescending { get; set; } = true;
}

public class CreateConversationRequest
{
    public int InboxId { get; set; }
    public int ContactId { get; set; }
    public string? InitialMessage { get; set; }
    public int? AssigneeId { get; set; }
    public int? TeamId { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Open;
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
