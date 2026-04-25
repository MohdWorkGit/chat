using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CustomerEngagement.Core.Entities;

public class CustomAttributeDefinition : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string AttributeDisplayName { get; set; }

    [MaxLength(50)]
    public string? AttributeDisplayType { get; set; }

    [MaxLength(1000)]
    public string? AttributeDescription { get; set; }

    [Required]
    [MaxLength(255)]
    public required string AttributeKey { get; set; }

    [Required]
    [MaxLength(50)]
    public required string AttributeModel { get; set; }

    public string? DefaultValue { get; set; }

    public string? ListValues { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;

    public List<string> GetListValues()
    {
        if (string.IsNullOrWhiteSpace(ListValues))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(ListValues) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public void SetListValues(List<string>? values)
    {
        ListValues = values is null || values.Count == 0
            ? null
            : JsonSerializer.Serialize(values);
    }
}
