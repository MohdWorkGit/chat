using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CaptainScenario : BaseEntity
{
    public int AssistantId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string Steps { get; set; } = "[]";

    // Navigation properties
    public CaptainAssistant Assistant { get; set; } = null!;
}
