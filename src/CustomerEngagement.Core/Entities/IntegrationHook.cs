using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class IntegrationHook : BaseEntity
{
    public int AccountId { get; set; }

    [MaxLength(255)]
    public string? AppId { get; set; }

    public int? InboxId { get; set; }
    public int Status { get; set; }

    [MaxLength(512)]
    public string? AccessToken { get; set; }

    [JsonPropertyName("settings")]
    public string? Settings { get; set; }

    [MaxLength(50)]
    public string? HookType { get; set; }

    [MaxLength(255)]
    public string? ReferenceId { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
