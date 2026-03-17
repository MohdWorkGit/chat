using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class Macro : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [JsonPropertyName("actions")]
    public string? Actions { get; set; }

    [MaxLength(50)]
    public string Visibility { get; set; } = "personal";

    public int? CreatedById { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
