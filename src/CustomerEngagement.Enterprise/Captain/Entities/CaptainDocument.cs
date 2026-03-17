using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CaptainDocument : BaseEntity
{
    public int AssistantId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string FileName { get; set; }

    [Required]
    [MaxLength(1024)]
    public required string FileUrl { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public CaptainAssistant Assistant { get; set; } = null!;
}
