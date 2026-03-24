namespace CustomerEngagement.Enterprise.CustomRoles.DTOs;

public record CustomRoleDto(
    int Id,
    int AccountId,
    string Name,
    string? Description,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateCustomRoleRequest(
    int AccountId,
    string Name,
    string? Description,
    List<string>? Permissions);

public record UpdateCustomRoleRequest(
    string Name,
    string? Description,
    List<string>? Permissions);
