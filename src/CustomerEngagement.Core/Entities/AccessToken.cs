using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class AccessToken : BaseEntity
{
    public int OwnerId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string OwnerType { get; set; }

    [Required]
    [MaxLength(512)]
    public required string Token { get; set; }

    // Navigation properties
    public User? Owner { get; set; }
}
