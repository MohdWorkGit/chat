using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CaptainAssistant : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public double Temperature { get; set; } = 0.7;

    [MaxLength(5000)]
    public string? ResponseGuidelines { get; set; }

    [MaxLength(5000)]
    public string? Guardrails { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<CaptainDocument> Documents { get; set; } = [];
    public ICollection<CaptainScenario> Scenarios { get; set; } = [];
    public ICollection<CaptainCustomTool> CustomTools { get; set; } = [];
    public ICollection<CaptainInbox> Inboxes { get; set; } = [];
}
