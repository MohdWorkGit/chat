using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Note : BaseEntity
{
    public int AccountId { get; set; }
    public int ContactId { get; set; }
    public int UserId { get; set; }

    [Required]
    public required string Content { get; set; }

    // Navigation properties
    public Contact Contact { get; set; } = null!;
    public User User { get; set; } = null!;
}
