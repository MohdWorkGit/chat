using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Conversation : BaseEntity
{
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

    [JsonPropertyName("additional_attributes")]
    public string? AdditionalAttributes { get; set; }

    [JsonPropertyName("custom_attributes")]
    public string? CustomAttributes { get; set; }

    public DateTime? SnoozedUntil { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public User? Assignee { get; set; }
    public Team? Team { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
}
