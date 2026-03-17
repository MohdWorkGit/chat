using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Contact : BaseEntity
{
    public int AccountId { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Identifier { get; set; }

    public ContactType ContactType { get; set; } = ContactType.Visitor;

    [JsonPropertyName("additional_attributes")]
    public string? AdditionalAttributes { get; set; }

    [JsonPropertyName("custom_attributes")]
    public string? CustomAttributes { get; set; }

    public DateTime? LastActivityAt { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<ContactInbox> ContactInboxes { get; set; } = [];
    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
}
