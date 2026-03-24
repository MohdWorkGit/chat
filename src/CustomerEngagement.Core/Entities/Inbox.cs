using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Inbox : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? ChannelType { get; set; }

    public int? ChannelId { get; set; }

    public bool GreetingEnabled { get; set; }
    public string? GreetingMessage { get; set; }
    public bool EnableAutoAssignment { get; set; } = true;
    public string? OutOfOfficeMessage { get; set; }
    public bool WorkingHoursEnabled { get; set; }
    public bool CsatSurveyEnabled { get; set; }

    [MaxLength(255)]
    public string? BusinessName { get; set; }

    public bool LockToSingleConversation { get; set; }

    public bool EnableEmailCollect { get; set; } = true;

    public bool AllowMessagesAfterResolved { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<InboxMember> InboxMembers { get; set; } = [];
    public ICollection<WorkingHour> WorkingHours { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<ContactInbox> ContactInboxes { get; set; } = [];

    /// <summary>
    /// True when auto-assignment is enabled for this inbox.
    /// </summary>
    public bool ShouldAutoAssign => EnableAutoAssignment;

    /// <summary>
    /// Determines whether the inbox is currently within working hours based on the
    /// provided UTC time and working hours collection.
    /// </summary>
    /// <param name="utcNow">The current UTC time to check against.</param>
    /// <param name="hours">The collection of working hours to evaluate. Must not be null.</param>
    /// <returns>True if working hours are disabled (always open) or the current time falls within working hours.</returns>
    public bool IsWithinWorkingHours(DateTime utcNow, ICollection<WorkingHour> hours)
    {
        ArgumentNullException.ThrowIfNull(hours);

        if (!WorkingHoursEnabled)
            return true;

        var dayOfWeek = (int)utcNow.DayOfWeek;
        var todayHours = hours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek && h.InboxId == Id);

        if (todayHours is null || todayHours.ClosedAllDay)
            return false;

        if (todayHours.OpenAllDay)
            return true;

        var currentTime = TimeOnly.FromDateTime(utcNow);
        var openTime = todayHours.GetOpenTime();
        var closeTime = todayHours.GetCloseTime();

        return currentTime >= openTime && currentTime <= closeTime;
    }
}
