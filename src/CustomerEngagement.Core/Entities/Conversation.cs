using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Conversation : BaseEntity
{
    private static readonly Dictionary<ConversationStatus, ConversationStatus[]> AllowedTransitions = new()
    {
        [ConversationStatus.Open] = [ConversationStatus.Resolved, ConversationStatus.Pending, ConversationStatus.Snoozed],
        [ConversationStatus.Resolved] = [ConversationStatus.Open],
        [ConversationStatus.Pending] = [ConversationStatus.Open, ConversationStatus.Resolved, ConversationStatus.Snoozed],
        [ConversationStatus.Snoozed] = [ConversationStatus.Open, ConversationStatus.Resolved],
    };

    public int AccountId { get; set; }
    public int InboxId { get; set; }
    public int ContactId { get; set; }
    public int? AssigneeId { get; set; }
    public int? TeamId { get; set; }
    public int DisplayId { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Open;
    public ConversationPriority Priority { get; set; } = ConversationPriority.None;

    [MaxLength(255)]
    public string? Identifier { get; set; }

    [MaxLength(255)]
    public string? Uuid { get; set; }

    [JsonPropertyName("additional_attributes")]
    public string? AdditionalAttributes { get; set; }

    [JsonPropertyName("custom_attributes")]
    public string? CustomAttributes { get; set; }

    public DateTime? SnoozedUntil { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public bool Muted { get; set; }
    public DateTime? WaitingSince { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public User? Assignee { get; set; }
    public Team? Team { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<ConversationParticipant> Participants { get; set; } = [];

    /// <summary>
    /// Checks whether a transition from the current status to the given status is valid.
    /// </summary>
    public bool CanTransitionTo(ConversationStatus newStatus)
    {
        if (Status == newStatus)
            return false;

        return AllowedTransitions.TryGetValue(Status, out var allowed) && allowed.Contains(newStatus);
    }

    /// <summary>
    /// Resolves the conversation. Only valid from Open, Pending, or Snoozed status.
    /// </summary>
    public void Resolve()
    {
        if (!CanTransitionTo(ConversationStatus.Resolved))
            throw new InvalidOperationException(
                $"Cannot resolve a conversation with status '{Status}'. Only Open, Pending, or Snoozed conversations can be resolved.");

        Status = ConversationStatus.Resolved;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reopens the conversation. Only valid from Resolved or Snoozed status.
    /// </summary>
    public void Reopen()
    {
        if (!CanTransitionTo(ConversationStatus.Open))
            throw new InvalidOperationException(
                $"Cannot reopen a conversation with status '{Status}'. Only Resolved or Snoozed conversations can be reopened.");

        Status = ConversationStatus.Open;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Snoozes the conversation until the specified time. Only valid from Open or Pending status.
    /// </summary>
    /// <param name="until">The UTC time to snooze until. Must be in the future.</param>
    public void Snooze(DateTime until)
    {
        if (until.Kind != DateTimeKind.Utc && until.Kind != DateTimeKind.Unspecified)
            throw new ArgumentException("Snooze time must be in UTC.", nameof(until));

        if (until <= DateTime.UtcNow)
            throw new ArgumentException("Snooze time must be in the future.", nameof(until));

        if (!CanTransitionTo(ConversationStatus.Snoozed))
            throw new InvalidOperationException(
                $"Cannot snooze a conversation with status '{Status}'. Only Open or Pending conversations can be snoozed.");

        Status = ConversationStatus.Snoozed;
        SnoozedUntil = until;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears the snooze and returns the conversation to Open status.
    /// </summary>
    public void Unsnooze()
    {
        if (Status != ConversationStatus.Snoozed)
            throw new InvalidOperationException(
                $"Cannot unsnooze a conversation with status '{Status}'. Only Snoozed conversations can be unsnoozed.");

        Status = ConversationStatus.Open;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns the conversation to an agent and/or team.
    /// </summary>
    /// <param name="agentId">The agent to assign to, or null to unassign.</param>
    /// <param name="teamId">The team to assign to, or null to unassign.</param>
    public void AssignTo(int? agentId, int? teamId)
    {
        AssigneeId = agentId;
        TeamId = teamId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Toggles the priority between None and Urgent.
    /// </summary>
    public void TogglePriority()
    {
        Priority = Priority == ConversationPriority.None
            ? ConversationPriority.Urgent
            : ConversationPriority.None;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mutes notifications for this conversation.
    /// </summary>
    public void Mute()
    {
        Muted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unmutes notifications for this conversation.
    /// </summary>
    public void Unmute()
    {
        Muted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a label to this conversation if it is not already present.
    /// </summary>
    /// <param name="label">The label to add. Must not be null.</param>
    public void AddLabel(Label label)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (Labels.Any(l => l.Id == label.Id))
            return;

        Labels.Add(label);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a label from this conversation.
    /// </summary>
    /// <param name="label">The label to remove. Must not be null.</param>
    public void RemoveLabel(Label label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var existing = Labels.FirstOrDefault(l => l.Id == label.Id);
        if (existing is not null)
        {
            Labels.Remove(existing);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
