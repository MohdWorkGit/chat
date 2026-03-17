using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Team : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool AllowAutoAssign { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<TeamMember> TeamMembers { get; set; } = [];
}
