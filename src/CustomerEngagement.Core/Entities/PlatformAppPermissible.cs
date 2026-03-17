using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class PlatformAppPermissible : BaseEntity
{
    public int PlatformAppId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string PermissibleType { get; set; }

    public int PermissibleId { get; set; }

    // Navigation properties
    public PlatformApp PlatformApp { get; set; } = null!;
}
