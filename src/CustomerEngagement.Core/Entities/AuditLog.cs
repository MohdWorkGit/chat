using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class AuditLog : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    [MaxLength(255)]
    public string? UserName { get; set; }

    [Required]
    [MaxLength(255)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string AuditableType { get; set; } = string.Empty;

    public int AuditableId { get; set; }

    public string? Changes { get; set; }

    [MaxLength(255)]
    public string? IpAddress { get; set; }

    [MaxLength(1024)]
    public string? UserAgent { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
