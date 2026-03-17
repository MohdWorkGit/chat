using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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

    [MaxLength(255)]
    [EmailAddress]
    public string? SupportEmail { get; set; }

    [MaxLength(255)]
    public string? BusinessName { get; set; }

    public bool DomainEmailsEnabled { get; set; }

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

    /// <summary>
    /// True when auto-resolve is configured with a positive number of days.
    /// </summary>
    public bool IsAutoResolveEnabled => AutoResolveAfterDays > 0;

    /// <summary>
    /// Checks whether the specified feature is enabled in the FeatureFlags JSON.
    /// The FeatureFlags JSON is expected to be a flat object with boolean values.
    /// </summary>
    /// <param name="featureName">The name of the feature to check. Must not be null or empty.</param>
    /// <returns>True if the feature is explicitly enabled; false otherwise.</returns>
    public bool HasFeature(string featureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        if (string.IsNullOrWhiteSpace(FeatureFlags))
            return false;

        try
        {
            var flags = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(FeatureFlags);
            if (flags is not null && flags.TryGetValue(featureName, out var value))
            {
                return value.ValueKind == JsonValueKind.True;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Enables a feature flag by name. Creates or updates the FeatureFlags JSON.
    /// </summary>
    /// <param name="featureName">The name of the feature to enable. Must not be null or empty.</param>
    public void EnableFeature(string featureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        var flags = DeserializeFlags();
        flags[featureName] = true;
        FeatureFlags = JsonSerializer.Serialize(flags);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disables a feature flag by name. Creates or updates the FeatureFlags JSON.
    /// </summary>
    /// <param name="featureName">The name of the feature to disable. Must not be null or empty.</param>
    public void DisableFeature(string featureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        var flags = DeserializeFlags();
        flags[featureName] = false;
        FeatureFlags = JsonSerializer.Serialize(flags);
        UpdatedAt = DateTime.UtcNow;
    }

    private Dictionary<string, bool> DeserializeFlags()
    {
        if (string.IsNullOrWhiteSpace(FeatureFlags))
            return new Dictionary<string, bool>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(FeatureFlags)
                   ?? new Dictionary<string, bool>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, bool>();
        }
    }
}
