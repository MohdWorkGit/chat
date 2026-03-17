using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? DisplayName { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(255)]
    public string? PasswordDigest { get; set; }

    public UserAvailability AvailabilityStatus { get; set; } = UserAvailability.Offline;

    [MaxLength(512)]
    public string? Avatar { get; set; }

    // Navigation properties
    public ICollection<AccountUser> AccountUsers { get; set; } = [];
}
