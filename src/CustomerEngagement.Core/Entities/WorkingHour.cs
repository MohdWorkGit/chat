namespace CustomerEngagement.Core.Entities;

public class WorkingHour : BaseEntity
{
    public int AccountId { get; set; }
    public int InboxId { get; set; }
    public int DayOfWeek { get; set; }
    public int OpenHour { get; set; }
    public int OpenMinutes { get; set; }
    public int CloseHour { get; set; }
    public int CloseMinutes { get; set; }
    public bool ClosedAllDay { get; set; }
    public bool OpenAllDay { get; set; }

    // Navigation properties
    public Inbox Inbox { get; set; } = null!;

    /// <summary>
    /// Returns the opening time as a TimeOnly value.
    /// </summary>
    public TimeOnly GetOpenTime() => new(OpenHour, OpenMinutes);

    /// <summary>
    /// Returns the closing time as a TimeOnly value.
    /// </summary>
    public TimeOnly GetCloseTime() => new(CloseHour, CloseMinutes);

    /// <summary>
    /// Checks if the given time falls within this working hour's open and close times.
    /// Returns false if the day is marked as closed all day, true if open all day.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns>True if the time falls within working hours.</returns>
    public bool IsOpenAt(TimeOnly time)
    {
        if (ClosedAllDay)
            return false;

        if (OpenAllDay)
            return true;

        return time >= GetOpenTime() && time <= GetCloseTime();
    }
}
