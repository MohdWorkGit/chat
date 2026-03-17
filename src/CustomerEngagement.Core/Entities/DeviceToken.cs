namespace CustomerEngagement.Core.Entities;

public class DeviceToken : BaseEntity
{
    public int UserId { get; set; }
    public required string Token { get; set; }
    public required string Platform { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
