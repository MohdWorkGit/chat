namespace CustomerEngagement.Api.Middleware;

/// <summary>
/// Adds standard HTTP security response headers (CSP, X-Frame-Options, etc.).
/// Configurable via the "Security:Headers" section. Setting "Csp" to empty disables CSP.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _csp;
    private readonly string _frameOptions;
    private readonly string _referrerPolicy;
    private readonly string _permissionsPolicy;
    private readonly bool _enableHsts;

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var section = configuration.GetSection("Security:Headers");
        _csp = section.GetValue<string?>("Csp",
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: blob:; " +
            "font-src 'self' data:; " +
            "connect-src 'self' ws: wss:; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");
        _frameOptions = section.GetValue<string>("FrameOptions", "DENY")!;
        _referrerPolicy = section.GetValue<string>("ReferrerPolicy", "strict-origin-when-cross-origin")!;
        _permissionsPolicy = section.GetValue<string>("PermissionsPolicy",
            "camera=(), microphone=(), geolocation=(), payment=()")!;
        _enableHsts = section.GetValue("EnableHsts", true);
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            if (!string.IsNullOrWhiteSpace(_csp) && !headers.ContainsKey("Content-Security-Policy"))
                headers["Content-Security-Policy"] = _csp;

            if (!headers.ContainsKey("X-Frame-Options"))
                headers["X-Frame-Options"] = _frameOptions;

            if (!headers.ContainsKey("X-Content-Type-Options"))
                headers["X-Content-Type-Options"] = "nosniff";

            if (!headers.ContainsKey("Referrer-Policy"))
                headers["Referrer-Policy"] = _referrerPolicy;

            if (!headers.ContainsKey("Permissions-Policy"))
                headers["Permissions-Policy"] = _permissionsPolicy;

            if (_enableHsts && context.Request.IsHttps && !headers.ContainsKey("Strict-Transport-Security"))
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            headers.Remove("X-Powered-By");
            headers.Remove("Server");

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
