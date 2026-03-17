using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class EmailTemplate : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Body { get; set; }

    [MaxLength(50)]
    public string? TemplateType { get; set; }

    [MaxLength(10)]
    public string? Locale { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
