using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.CustomRoles.Entities;

public class CustomRoleAssignment : BaseEntity
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public int CustomRoleId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public CustomRole CustomRole { get; set; } = null!;
}
