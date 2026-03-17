using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class InstallationConfig : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Value { get; set; }

    public bool Locked { get; set; }
}
