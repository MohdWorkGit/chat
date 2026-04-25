using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Attachment : BaseEntity
{
    public int MessageId { get; set; }
    public int AccountId { get; set; }
    public AttachmentType FileType { get; set; } = AttachmentType.File;

    /// <summary>
    /// Opaque storage identifier (e.g. S3 object key). Readers turn this
    /// into a resolvable URL via <c>IStorageService.GetFileUrlAsync</c>.
    /// For externally-hosted files this may also hold an absolute URL;
    /// the projection returns it unchanged in that case.
    /// </summary>
    [MaxLength(2048)]
    public string? ExternalUrl { get; set; }

    [MaxLength(255)]
    public string? FileName { get; set; }

    public long FileSize { get; set; }

    [MaxLength(255)]
    public string? ContentType { get; set; }

    [MaxLength(2048)]
    public string? ThumbnailUrl { get; set; }

    public double? CoordinatesLat { get; set; }
    public double? CoordinatesLon { get; set; }

    [MaxLength(255)]
    public string? FallbackTitle { get; set; }

    [MaxLength(50)]
    public string? Extension { get; set; }

    // Navigation properties
    public Message Message { get; set; } = null!;
}
