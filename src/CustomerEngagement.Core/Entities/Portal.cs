using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Portal : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Slug { get; set; }

    [MaxLength(255)]
    public string? CustomDomain { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [MaxLength(500)]
    public string? HeaderText { get; set; }

    [MaxLength(255)]
    public string? PageTitle { get; set; }

    [MaxLength(2048)]
    public string? HomepageLink { get; set; }

    public bool Archived { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<Article> Articles { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
}
