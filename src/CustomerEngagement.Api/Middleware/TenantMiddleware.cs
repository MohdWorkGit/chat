using System.Security.Claims;

namespace CustomerEngagement.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> SkipPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth",
        "/health",
        "/hubs",
        "/hangfire",
        "/swagger"
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        long? accountId = null;

        // 1. Try route values
        if (context.Request.RouteValues.TryGetValue("accountId", out var routeAccountId)
            && long.TryParse(routeAccountId?.ToString(), out var parsedRoute))
        {
            accountId = parsedRoute;
        }

        // 2. Try header
        if (accountId is null
            && context.Request.Headers.TryGetValue("X-Account-Id", out var headerValue)
            && long.TryParse(headerValue.FirstOrDefault(), out var parsedHeader))
        {
            accountId = parsedHeader;
        }

        // 3. Try JWT claim
        if (accountId is null)
        {
            var claim = context.User.FindFirst("account_id")
                ?? context.User.FindFirst("AccountId");
            if (claim is not null && long.TryParse(claim.Value, out var parsedClaim))
            {
                accountId = parsedClaim;
            }
        }

        if (accountId.HasValue)
        {
            context.Items["AccountId"] = accountId.Value;
        }

        await _next(context);
    }
}
