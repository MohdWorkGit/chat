using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class CustomAttributeDefinition : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string AttributeDisplayName { get; set; }

    [MaxLength(50)]
    public string? AttributeDisplayType { get; set; }

    [MaxLength(1000)]
    public string? AttributeDescription { get; set; }

    [Required]
    [MaxLength(255)]
    public required string AttributeKey { get; set; }

    [Required]
    [MaxLength(50)]
    public required string AttributeModel { get; set; }

    public string? DefaultValue { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
