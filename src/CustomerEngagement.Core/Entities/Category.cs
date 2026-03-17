using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Category : BaseEntity
{
    public int AccountId { get; set; }
    public int PortalId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Slug { get; set; }

    public int Position { get; set; }

    [MaxLength(10)]
    public string? Locale { get; set; }

    public int? ParentCategoryId { get; set; }

    // Navigation properties
    public Portal Portal { get; set; } = null!;
    public ICollection<Article> Articles { get; set; } = [];
    public Category? ParentCategory { get; set; }
}
