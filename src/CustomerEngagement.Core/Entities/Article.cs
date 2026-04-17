using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Article : BaseEntity
{
    public int AccountId { get; set; }
    public int PortalId { get; set; }
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    public string? Content { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Slug { get; set; }

    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public int Position { get; set; }
    public int ViewCount { get; set; }

    [MaxLength(10)]
    public string? Locale { get; set; }

    // Navigation properties
    public Portal Portal { get; set; } = null!;
    public Category? Category { get; set; }
    public User? Author { get; set; }
}
