using System.Text.Json;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Enterprise.CustomRoles.DTOs;
using CustomerEngagement.Enterprise.CustomRoles.Entities;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.CustomRoles.Services;

public class CustomRoleService : ICustomRoleService
{
    private readonly IRepository<CustomRole> _roleRepository;
    private readonly IRepository<CustomRoleAssignment> _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomRoleService> _logger;

    public CustomRoleService(
        IRepository<CustomRole> roleRepository,
        IRepository<CustomRoleAssignment> assignmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CustomRoleService> logger)
    {
        _roleRepository = roleRepository;
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomRoleDto>> GetRolesAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.FindAsync(r => r.AccountId == accountId, cancellationToken);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<CustomRoleDto?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        return role is null ? null : MapToDto(role);
    }

    public async Task<CustomRoleDto> CreateRoleAsync(CreateCustomRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = new CustomRole
        {
            AccountId = request.AccountId,
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions is { Count: > 0 }
                ? JsonSerializer.Serialize(request.Permissions)
                : null
        };

        var created = await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created custom role {RoleId} for account {AccountId}", created.Id, request.AccountId);

        return MapToDto(created);
    }

    public async Task<CustomRoleDto> UpdateRoleAsync(int roleId, UpdateCustomRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Custom role with id {roleId} not found.");

        role.Name = request.Name;
        role.Description = request.Description;
        role.Permissions = request.Permissions is not null
            ? JsonSerializer.Serialize(request.Permissions)
            : role.Permissions;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated custom role {RoleId}", roleId);

        return MapToDto(role);
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Custom role with id {roleId} not found.");

        // Remove all assignments for this role
        var assignments = await _assignmentRepository.FindAsync(a => a.CustomRoleId == roleId, cancellationToken);
        foreach (var assignment in assignments)
        {
            await _assignmentRepository.DeleteAsync(assignment, cancellationToken);
        }

        await _roleRepository.DeleteAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted custom role {RoleId}", roleId);
    }

    public async Task AssignRoleToUserAsync(int accountId, int userId, int roleId, CancellationToken cancellationToken = default)
    {
        // Verify the role exists and belongs to the account
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Custom role with id {roleId} not found.");

        if (role.AccountId != accountId)
        {
            throw new InvalidOperationException("Role does not belong to the specified account.");
        }

        // Check if assignment already exists
        var existing = await _assignmentRepository.FindAsync(
            a => a.AccountId == accountId && a.UserId == userId && a.CustomRoleId == roleId,
            cancellationToken);

        if (existing.Count > 0)
        {
            _logger.LogWarning("User {UserId} already has role {RoleId} assigned", userId, roleId);
            return;
        }

        var assignment = new CustomRoleAssignment
        {
            AccountId = accountId,
            UserId = userId,
            CustomRoleId = roleId
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned role {RoleId} to user {UserId} in account {AccountId}", roleId, userId, accountId);
    }

    public async Task RemoveRoleFromUserAsync(int accountId, int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentRepository.FindAsync(
            a => a.AccountId == accountId && a.UserId == userId && a.CustomRoleId == roleId,
            cancellationToken);

        foreach (var assignment in assignments)
        {
            await _assignmentRepository.DeleteAsync(assignment, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed role {RoleId} from user {UserId} in account {AccountId}", roleId, userId, accountId);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(int accountId, int userId, CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentRepository.FindAsync(
            a => a.AccountId == accountId && a.UserId == userId,
            cancellationToken);

        var allPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assignment in assignments)
        {
            var role = await _roleRepository.GetByIdAsync(assignment.CustomRoleId, cancellationToken);
            if (role is not null)
            {
                foreach (var permission in role.GetPermissions())
                {
                    allPermissions.Add(permission);
                }
            }
        }

        return allPermissions.ToList();
    }

    private static CustomRoleDto MapToDto(CustomRole role)
    {
        return new CustomRoleDto(
            role.Id,
            role.AccountId,
            role.Name,
            role.Description,
            role.GetPermissions(),
            role.CreatedAt,
            role.UpdatedAt);
    }
}
