using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class PlatformApp : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<PlatformAppPermissible> PlatformAppPermissibles { get; set; } = [];
}
