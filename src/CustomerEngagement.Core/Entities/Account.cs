using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class Account : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(10)]
    public string? Locale { get; set; }

    [MaxLength(255)]
    public string? Domain { get; set; }

    [JsonPropertyName("feature_flags")]
    public string? FeatureFlags { get; set; }

    public int AutoResolveAfterDays { get; set; } = 14;

    // Navigation properties
    public ICollection<AccountUser> Users { get; set; } = [];
    public ICollection<Inbox> Inboxes { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<Campaign> Campaigns { get; set; } = [];
    public ICollection<CannedResponse> CannedResponses { get; set; } = [];
    public ICollection<Webhook> Webhooks { get; set; } = [];
    public ICollection<AutomationRule> AutomationRules { get; set; } = [];
    public ICollection<CustomAttributeDefinition> CustomAttributeDefinitions { get; set; } = [];
    public ICollection<Portal> Portals { get; set; } = [];
    public ICollection<EmailTemplate> EmailTemplates { get; set; } = [];
    public ICollection<IntegrationHook> IntegrationHooks { get; set; } = [];
}
