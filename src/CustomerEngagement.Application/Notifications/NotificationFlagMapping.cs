using System.Text.Json;

namespace CustomerEngagement.Application.Notifications;

/// <summary>
/// Helpers for translating between the stored notification flag strings
/// (jsonb arrays) and the boolean toggles surfaced to the dashboard.
/// The stored representation remains compatible with the substring
/// matching used by <c>NotificationDeliveryJob</c>.
/// </summary>
internal static class NotificationFlagMapping
{
    internal const string ConversationCreationFlag = "conversation_creation";
    internal const string ConversationAssignmentFlag = "conversation_assignment";
    // Matches both assigned_conversation_new_message and
    // participating_conversation_new_message via substring comparison.
    internal const string NewMessageFlag = "new_message";
    internal const string MentionFlag = "mention";

    internal sealed class Flags
    {
        public bool ConversationCreation { get; set; } = true;
        public bool ConversationAssignment { get; set; } = true;
        public bool NewMessage { get; set; } = true;
        public bool Mention { get; set; } = true;
    }

    internal static Flags ParseEmail(string? raw) => Parse(raw);

    internal static Flags ParsePush(string? raw) => Parse(raw);

    private static Flags Parse(string? raw)
    {
        // Null/empty means "all enabled" per NotificationDeliveryJob semantics.
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new Flags();
        }

        // Flags are stored as a JSON array of flag identifiers.
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
            return new Flags
            {
                ConversationCreation = list.Contains(ConversationCreationFlag),
                ConversationAssignment = list.Contains(ConversationAssignmentFlag),
                NewMessage = list.Contains(NewMessageFlag),
                Mention = list.Contains(MentionFlag),
            };
        }
        catch (JsonException)
        {
            // Fallback: treat the raw value as a comma-separated string.
            return new Flags
            {
                ConversationCreation = raw.Contains(ConversationCreationFlag, StringComparison.OrdinalIgnoreCase),
                ConversationAssignment = raw.Contains(ConversationAssignmentFlag, StringComparison.OrdinalIgnoreCase),
                NewMessage = raw.Contains(NewMessageFlag, StringComparison.OrdinalIgnoreCase),
                Mention = raw.Contains(MentionFlag, StringComparison.OrdinalIgnoreCase),
            };
        }
    }

    internal static string Serialize(Flags flags)
    {
        var list = new List<string>(4);
        if (flags.ConversationCreation) list.Add(ConversationCreationFlag);
        if (flags.ConversationAssignment) list.Add(ConversationAssignmentFlag);
        if (flags.NewMessage) list.Add(NewMessageFlag);
        if (flags.Mention) list.Add(MentionFlag);
        return JsonSerializer.Serialize(list);
    }
}
