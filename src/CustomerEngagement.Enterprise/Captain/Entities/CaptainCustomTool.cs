using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CaptainCustomTool : BaseEntity
{
    public int AssistantId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string Parameters { get; set; } = "{}";

    [Required]
    [MaxLength(1024)]
    public required string EndpointUrl { get; set; }

    // Navigation properties
    public CaptainAssistant Assistant { get; set; } = null!;
}
