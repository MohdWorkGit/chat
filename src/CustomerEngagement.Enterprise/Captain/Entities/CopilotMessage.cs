using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CopilotMessage : BaseEntity
{
    public int ThreadId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Role { get; set; }

    [Required]
    public required string Content { get; set; }

    // Navigation properties
    public CopilotThread Thread { get; set; } = null!;
}
