using CustomerEngagement.Application.Auth;
using CustomerEngagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(string name, string email, string password, int accountId, string role = "Agent")
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration failed: email {Email} already exists", email);
            return new AuthResult(false, null, null, new[] { "A user with this email already exists." });
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            Name = name,
            Provider = "email",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            _logger.LogWarning("Registration failed for {Email}: {Errors}", email, string.Join(", ", errors));
            return new AuthResult(false, null, null, errors);
        }

        await _userManager.AddToRoleAsync(user, role);

        var accessToken = _jwtTokenService.GenerateAccessToken(user, accountId, role);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} registered successfully", email);

        return new AuthResult(true, accessToken, refreshToken);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, int accountId)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user {Email} not found", email);
            return new AuthResult(false, null, null, new[] { "Invalid email or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: user {Email} is locked out", email);
                return new AuthResult(false, null, null, new[] { "Account is locked. Please try again later." });
            }

            _logger.LogWarning("Login failed: invalid password for {Email}", email);
            return new AuthResult(false, null, null, new[] { "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Agent";

        var accessToken = _jwtTokenService.GenerateAccessToken(user, accountId, role);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} logged in successfully", email);

        return new AuthResult(true, accessToken, refreshToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken, int accountId)
    {
        var principal = _jwtTokenService.ValidateToken(accessToken);
        if (principal is null)
        {
            return new AuthResult(false, null, null, new[] { "Invalid access token." });
        }

        var userIdClaim = principal.FindFirst("UserId")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return new AuthResult(false, null, null, new[] { "Invalid token claims." });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new AuthResult(false, null, null, new[] { "User not found." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Agent";

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, accountId, role);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResult(true, newAccessToken, newRefreshToken);
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (false, new[] { "User not found." });
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description));
        }

        _logger.LogInformation("Password reset successfully for {Email}", email);
        return (true, null);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Password reset token requested for unknown email {Email}", email);
            return null;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Password reset token generated for {Email}", email);
        return token;
    }
}
