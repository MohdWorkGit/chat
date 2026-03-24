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

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    [MaxLength(20)]
    public string? BrowserLanguage { get; set; }

    [MaxLength(10)]
    public string? CountryCode { get; set; }

    [MaxLength(255)]
    public string? CompanyName { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<ContactInbox> ContactInboxes { get; set; } = [];
    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];

    /// <summary>
    /// True when the contact has a non-empty email address.
    /// </summary>
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// True when the contact has a non-empty phone number.
    /// </summary>
    public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);

    /// <summary>
    /// Records the current time as the last activity timestamp.
    /// </summary>
    public void RecordActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Merges non-null fields from another contact into this contact using null-coalescing.
    /// The source contact must belong to the same account.
    /// </summary>
    /// <param name="other">The contact to merge fields from. Must not be null.</param>
    public void MergeFrom(Contact other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Id == Id)
            throw new ArgumentException("Cannot merge a contact with itself.", nameof(other));

        Name ??= other.Name;
        Email ??= other.Email;
        Phone ??= other.Phone;
        Identifier ??= other.Identifier;
        AdditionalAttributes ??= other.AdditionalAttributes;
        CustomAttributes ??= other.CustomAttributes;
        LastActivityAt ??= other.LastActivityAt;
        IpAddress ??= other.IpAddress;
        Location ??= other.Location;
        BrowserLanguage ??= other.BrowserLanguage;
        CountryCode ??= other.CountryCode;
        CompanyName ??= other.CompanyName;

        UpdatedAt = DateTime.UtcNow;
    }
}
