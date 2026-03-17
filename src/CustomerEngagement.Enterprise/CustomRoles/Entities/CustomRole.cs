using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.CustomRoles.Entities;

public class CustomRole : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? Permissions { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;

    public bool HasPermission(string permission)
    {
        var permissions = GetPermissions();
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public void AddPermission(string permission)
    {
        var permissions = GetPermissions();
        if (!permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
        {
            permissions.Add(permission);
            Permissions = JsonSerializer.Serialize(permissions);
        }
    }

    public void RemovePermission(string permission)
    {
        var permissions = GetPermissions();
        permissions.RemoveAll(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase));
        Permissions = JsonSerializer.Serialize(permissions);
    }

    public List<string> GetPermissions()
    {
        if (string.IsNullOrWhiteSpace(Permissions))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(Permissions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
