using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Attachment : BaseEntity
{
    public int MessageId { get; set; }
    public int AccountId { get; set; }
    public AttachmentType FileType { get; set; } = AttachmentType.File;

    [MaxLength(2048)]
    public string? ExternalUrl { get; set; }

    public double? CoordinatesLat { get; set; }
    public double? CoordinatesLon { get; set; }

    [MaxLength(255)]
    public string? FallbackTitle { get; set; }

    [MaxLength(50)]
    public string? Extension { get; set; }

    // Navigation properties
    public Message Message { get; set; } = null!;
}
