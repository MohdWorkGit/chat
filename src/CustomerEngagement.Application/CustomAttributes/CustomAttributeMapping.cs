using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Application.CustomAttributes;

internal static class CustomAttributeMapping
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text", "number", "date", "list", "checkbox", "link", "currency"
    };

    private static readonly HashSet<string> ValidAppliedTo = new(StringComparer.OrdinalIgnoreCase)
    {
        "contact", "conversation"
    };

    public static string ValidateType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type) || !ValidTypes.Contains(type))
            throw new ArgumentException(
                $"Attribute type must be one of: {string.Join(", ", ValidTypes)}.",
                nameof(type));
        return type.ToLowerInvariant();
    }

    public static string ValidateAppliedTo(string? appliedTo)
    {
        if (string.IsNullOrWhiteSpace(appliedTo) || !ValidAppliedTo.Contains(appliedTo))
            throw new ArgumentException(
                $"Applied-to must be one of: {string.Join(", ", ValidAppliedTo)}.",
                nameof(appliedTo));
        return appliedTo.ToLowerInvariant();
    }

    public static bool IsListType(string? type) =>
        string.Equals(type, "list", StringComparison.OrdinalIgnoreCase);

    public static object ToDto(CustomAttributeDefinition entity)
    {
        return new
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            DisplayName = entity.AttributeDisplayName,
            Key = entity.AttributeKey,
            AttributeType = entity.AttributeDisplayType,
            AppliedTo = entity.AttributeModel,
            Description = entity.AttributeDescription,
            ListValues = IsListType(entity.AttributeDisplayType) ? entity.GetListValues() : null,
            entity.CreatedAt,
            entity.UpdatedAt
        };
    }
}
