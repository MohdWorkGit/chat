using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class Webhook : BaseEntity
{
    public int AccountId { get; set; }
    public int? InboxId { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    public required string Url { get; set; }

    [JsonPropertyName("subscribed_events")]
    public string? SubscribedEvents { get; set; }

    [MaxLength(255)]
    public string? HmacToken { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;

    /// <summary>
    /// Deserializes the SubscribedEvents JSON into a list of event name strings.
    /// Returns an empty list if SubscribedEvents is null or empty.
    /// </summary>
    public List<string> GetSubscribedEvents()
    {
        if (string.IsNullOrWhiteSpace(SubscribedEvents))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(SubscribedEvents) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Serializes the given list of event names to JSON and stores it in SubscribedEvents.
    /// </summary>
    /// <param name="events">The list of event names to subscribe to. Must not be null.</param>
    public void SetSubscribedEvents(List<string> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        SubscribedEvents = JsonSerializer.Serialize(events);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks whether the webhook is subscribed to the specified event.
    /// </summary>
    /// <param name="eventName">The event name to check. Must not be null or empty.</param>
    /// <returns>True if the webhook is subscribed to the event; false otherwise.</returns>
    public bool IsSubscribedTo(string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return GetSubscribedEvents().Contains(eventName, StringComparer.OrdinalIgnoreCase);
    }
}
