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
}
