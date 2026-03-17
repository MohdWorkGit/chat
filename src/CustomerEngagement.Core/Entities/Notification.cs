using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Notification : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    [MaxLength(100)]
    public string? NotificationType { get; set; }

    [MaxLength(100)]
    public string? PrimaryActorType { get; set; }

    public int? PrimaryActorId { get; set; }

    [MaxLength(100)]
    public string? SecondaryActorType { get; set; }

    public int? SecondaryActorId { get; set; }

    public DateTime? ReadAt { get; set; }
    public DateTime? SnoozedUntil { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    /// <summary>
    /// True when the notification has been read.
    /// </summary>
    public bool IsRead => ReadAt.HasValue;

    /// <summary>
    /// True when the notification is currently snoozed (snoozed until a future time).
    /// </summary>
    public bool IsSnoozed => SnoozedUntil.HasValue && SnoozedUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Marks the notification as read at the current UTC time.
    /// </summary>
    public void MarkRead()
    {
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the notification as unread by clearing the read timestamp.
    /// </summary>
    public void MarkUnread()
    {
        ReadAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Snoozes the notification until the specified UTC time.
    /// </summary>
    /// <param name="until">The UTC time to snooze until. Must be in the future.</param>
    public void Snooze(DateTime until)
    {
        if (until <= DateTime.UtcNow)
            throw new ArgumentException("Snooze time must be in the future.", nameof(until));

        SnoozedUntil = until;
        UpdatedAt = DateTime.UtcNow;
    }
}
