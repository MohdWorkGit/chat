using CustomerEngagement.Enterprise.CustomRoles.DTOs;
using CustomerEngagement.Enterprise.CustomRoles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Enterprise;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/custom_roles")]
[Authorize]
public class CustomRolesController : ControllerBase
{
    private readonly ICustomRoleService _customRoleService;

    public CustomRolesController(ICustomRoleService customRoleService)
    {
        _customRoleService = customRoleService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomRoleDto>>> GetAll(
        int accountId,
        CancellationToken cancellationToken)
    {
        var roles = await _customRoleService.GetRolesAsync(accountId, cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{roleId:int}")]
    public async Task<ActionResult<CustomRoleDto>> GetById(
        int accountId,
        int roleId,
        CancellationToken cancellationToken)
    {
        var role = await _customRoleService.GetRoleByIdAsync(roleId, cancellationToken);
        if (role is null || role.AccountId != accountId)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<CustomRoleDto>> Create(
        int accountId,
        [FromBody] CreateCustomRoleBody body,
        CancellationToken cancellationToken)
    {
        var request = new CreateCustomRoleRequest(accountId, body.Name, body.Description, body.Permissions);
        var created = await _customRoleService.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { accountId, roleId = created.Id }, created);
    }

    [HttpPatch("{roleId:int}")]
    public async Task<ActionResult<CustomRoleDto>> Update(
        int accountId,
        int roleId,
        [FromBody] UpdateCustomRoleRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _customRoleService.GetRoleByIdAsync(roleId, cancellationToken);
        if (existing is null || existing.AccountId != accountId)
        {
            return NotFound();
        }

        var updated = await _customRoleService.UpdateRoleAsync(roleId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{roleId:int}")]
    public async Task<ActionResult> Delete(
        int accountId,
        int roleId,
        CancellationToken cancellationToken)
    {
        var existing = await _customRoleService.GetRoleByIdAsync(roleId, cancellationToken);
        if (existing is null || existing.AccountId != accountId)
        {
            return NotFound();
        }

        await _customRoleService.DeleteRoleAsync(roleId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{roleId:int}/assignments")]
    public async Task<ActionResult> AssignRole(
        int accountId,
        int roleId,
        [FromBody] AssignRoleBody body,
        CancellationToken cancellationToken)
    {
        await _customRoleService.AssignRoleToUserAsync(accountId, body.UserId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{roleId:int}/assignments/{userId:int}")]
    public async Task<ActionResult> RemoveAssignment(
        int accountId,
        int roleId,
        int userId,
        CancellationToken cancellationToken)
    {
        await _customRoleService.RemoveRoleFromUserAsync(accountId, userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("/api/v1/accounts/{accountId:int}/users/{userId:int}/permissions")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetUserPermissions(
        int accountId,
        int userId,
        CancellationToken cancellationToken)
    {
        var permissions = await _customRoleService.GetUserPermissionsAsync(accountId, userId, cancellationToken);
        return Ok(permissions);
    }
}

public record CreateCustomRoleBody(string Name, string? Description, List<string>? Permissions);

public record AssignRoleBody(int UserId);
