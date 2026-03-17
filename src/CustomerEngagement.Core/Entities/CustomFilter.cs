using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class CustomFilter : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(50)]
    public string? FilterType { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
