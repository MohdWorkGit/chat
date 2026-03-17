using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class CannedResponse : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string ShortCode { get; set; }

    [Required]
    public required string Content { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
