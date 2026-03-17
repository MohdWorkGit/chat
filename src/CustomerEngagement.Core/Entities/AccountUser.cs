using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class AccountUser : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }
    public UserRole Role { get; set; } = UserRole.Agent;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
