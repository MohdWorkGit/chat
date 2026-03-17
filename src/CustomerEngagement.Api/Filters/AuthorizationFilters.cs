using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomerEngagement.Api.Filters;

/// <summary>
/// Verifies that the current user has the required role for the specified account.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AccountAuthorizeAttribute : TypeFilterAttribute
{
    public AccountAuthorizeAttribute(params string[] roles)
        : base(typeof(AccountAuthorizationFilter))
    {
        Arguments = new object[] { roles };
    }
}

public class AccountAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string[] _requiredRoles;
    private readonly ILogger<AccountAuthorizationFilter> _logger;

    public AccountAuthorizationFilter(string[] roles, ILogger<AccountAuthorizationFilter> logger)
    {
        _requiredRoles = roles;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.Items.TryGetValue("AccountId", out var accountIdObj)
            || accountIdObj is not long accountId)
        {
            _logger.LogWarning("AccountId not found in HttpContext.Items for authorization check.");
            context.Result = new ForbidResult();
            return;
        }

        // Check account_id claim matches the route account
        var accountClaim = user.FindFirst("account_id")?.Value
            ?? user.FindFirst("AccountId")?.Value;

        if (accountClaim is null || !long.TryParse(accountClaim, out var claimAccountId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Super admins bypass account check
        if (user.IsInRole("SuperAdmin"))
        {
            return;
        }

        if (claimAccountId != accountId)
        {
            _logger.LogWarning("Account mismatch: claim={ClaimAccountId}, route={RouteAccountId}",
                claimAccountId, accountId);
            context.Result = new ForbidResult();
            return;
        }

        // Check role if specific roles are required
        if (_requiredRoles.Length > 0)
        {
            var userRole = user.FindFirst("account_role")?.Value
                ?? user.FindFirst("AccountRole")?.Value;

            if (userRole is null || !_requiredRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User role {Role} not in required roles [{Roles}]",
                    userRole, string.Join(", ", _requiredRoles));
                context.Result = new ForbidResult();
                return;
            }
        }

        await Task.CompletedTask;
    }
}
