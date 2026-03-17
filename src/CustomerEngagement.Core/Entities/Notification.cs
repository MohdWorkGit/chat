using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Notification : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    [MaxLength(100)]
    public string? NotificationType { get; set; }

    [MaxLength(100)]
    public string? PrimaryActorType { get; set; }

    public int? PrimaryActorId { get; set; }

    [MaxLength(100)]
    public string? SecondaryActorType { get; set; }

    public int? SecondaryActorId { get; set; }

    public DateTime? ReadAt { get; set; }
    public DateTime? SnoozedUntil { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
