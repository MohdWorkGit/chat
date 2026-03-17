using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class AutomationRule : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public required string EventName { get; set; }

    [JsonPropertyName("conditions")]
    public string? Conditions { get; set; }

    [JsonPropertyName("actions")]
    public string? Actions { get; set; }

    public bool Active { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;

    /// <summary>
    /// Activates this automation rule.
    /// </summary>
    public void Activate()
    {
        if (Active)
            throw new InvalidOperationException("The automation rule is already active.");

        Active = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this automation rule.
    /// </summary>
    public void Deactivate()
    {
        if (!Active)
            throw new InvalidOperationException("The automation rule is already inactive.");

        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deserializes the Conditions JSON string into a list of AutomationCondition records.
    /// Returns an empty list if Conditions is null or empty.
    /// </summary>
    public List<AutomationCondition> GetConditions()
    {
        if (string.IsNullOrWhiteSpace(Conditions))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<AutomationCondition>>(Conditions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Serializes the given list of conditions to JSON and stores it in the Conditions property.
    /// </summary>
    /// <param name="conditions">The conditions to set. Must not be null.</param>
    public void SetConditions(List<AutomationCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        Conditions = JsonSerializer.Serialize(conditions);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deserializes the Actions JSON string into a list of AutomationAction records.
    /// Returns an empty list if Actions is null or empty.
    /// </summary>
    public List<AutomationAction> GetActions()
    {
        if (string.IsNullOrWhiteSpace(Actions))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<AutomationAction>>(Actions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Serializes the given list of actions to JSON and stores it in the Actions property.
    /// </summary>
    /// <param name="actions">The actions to set. Must not be null.</param>
    public void SetActions(List<AutomationAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        Actions = JsonSerializer.Serialize(actions);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a single condition in an automation rule.
    /// </summary>
    public record AutomationCondition(
        [property: JsonPropertyName("attribute_key")] string AttributeKey,
        [property: JsonPropertyName("filter_operator")] string FilterOperator,
        [property: JsonPropertyName("values")] List<string> Values,
        [property: JsonPropertyName("query_operator")] string? QueryOperator = null
    );

    /// <summary>
    /// Represents a single action in an automation rule.
    /// </summary>
    public record AutomationAction(
        [property: JsonPropertyName("action_name")] string ActionName,
        [property: JsonPropertyName("action_params")] List<string> ActionParams
    );
}
