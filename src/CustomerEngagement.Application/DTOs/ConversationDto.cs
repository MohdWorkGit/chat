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
