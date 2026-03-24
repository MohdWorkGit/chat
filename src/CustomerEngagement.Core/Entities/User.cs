using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace CustomerEngagement.Core.Entities;

public class User : IdentityUser<int>
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? DisplayName { get; set; }

    [MaxLength(255)]
    public string? PasswordDigest { get; set; }

    public UserAvailability AvailabilityStatus { get; set; } = UserAvailability.Offline;

    [MaxLength(512)]
    public string? Avatar { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    [MaxLength(255)]
    public string? Uid { get; set; }

    public string? CustomAttributes { get; set; }

    public string? MessageSignature { get; set; }

    public int Availability { get; set; }

    [MaxLength(50)]
    public string? Type { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public string? UiSettings { get; set; }

    [MaxLength(255)]
    public string? Provider { get; set; }

    [MaxLength(255)]
    public string? AccessToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<AccountUser> AccountUsers { get; set; } = [];
    public ICollection<Conversation> AssignedConversations { get; set; } = [];
    public ICollection<InboxMember> InboxMembers { get; set; } = [];
    public ICollection<TeamMember> TeamMembers { get; set; } = [];
    public ICollection<Mention> Mentions { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];

    /// <summary>
    /// True when the user's availability status is Online.
    /// </summary>
    public bool IsAvailable => AvailabilityStatus == UserAvailability.Online;

    /// <summary>
    /// Sets the user's availability status.
    /// </summary>
    /// <param name="status">The new availability status.</param>
    public void SetAvailability(UserAvailability status)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException($"Invalid availability status: {status}.", nameof(status));

        AvailabilityStatus = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks whether the user has the Administrator role for the specified account.
    /// Requires the AccountUsers navigation property to be loaded.
    /// </summary>
    /// <param name="accountId">The account ID to check against.</param>
    /// <returns>True if the user is an administrator for the given account.</returns>
    public bool IsAdministrator(int accountId)
    {
        return AccountUsers.Any(au =>
            au.AccountId == accountId && au.Role == UserRole.Administrator);
    }
}
