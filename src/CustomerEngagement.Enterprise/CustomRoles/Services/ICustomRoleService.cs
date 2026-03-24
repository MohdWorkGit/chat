using CustomerEngagement.Enterprise.CustomRoles.DTOs;
using CustomerEngagement.Enterprise.CustomRoles.Entities;

namespace CustomerEngagement.Enterprise.CustomRoles.Services;

public interface ICustomRoleService
{
    Task<IReadOnlyList<CustomRoleDto>> GetRolesAsync(int accountId, CancellationToken cancellationToken = default);
    Task<CustomRoleDto?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<CustomRoleDto> CreateRoleAsync(CreateCustomRoleRequest request, CancellationToken cancellationToken = default);
    Task<CustomRoleDto> UpdateRoleAsync(int roleId, UpdateCustomRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(int accountId, int userId, int roleId, CancellationToken cancellationToken = default);
    Task RemoveRoleFromUserAsync(int accountId, int userId, int roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(int accountId, int userId, CancellationToken cancellationToken = default);
}
