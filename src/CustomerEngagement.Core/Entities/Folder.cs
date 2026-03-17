using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Folder : BaseEntity
{
    public int AccountId { get; set; }
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
}
