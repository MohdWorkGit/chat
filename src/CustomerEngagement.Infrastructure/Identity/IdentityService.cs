using CustomerEngagement.Application.Auth;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _dbContext;

    public IdentityService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<IdentityService> logger,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Resolves the effective role for a user within a specific account.
    /// Prefers the account-scoped <see cref="AccountUser.Role"/> when present,
    /// then falls back to the global ASP.NET Identity role, then "Agent".
    /// </summary>
    private async Task<string> ResolveAccountRoleAsync(User user, int accountId)
    {
        var accountUser = await _dbContext.AccountUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.UserId == user.Id && au.AccountId == accountId);

        if (accountUser is not null)
        {
            return accountUser.Role.ToString();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? nameof(UserRole.Agent);
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

        // Ensure an AccountUser exists so the role is bound to this account.
        var hasAccountUser = await _dbContext.AccountUsers
            .AnyAsync(au => au.UserId == user.Id && au.AccountId == accountId);
        if (!hasAccountUser)
        {
            var parsedRole = Enum.TryParse<UserRole>(role, ignoreCase: true, out var r) ? r : UserRole.Agent;
            _dbContext.AccountUsers.Add(new AccountUser
            {
                AccountId = accountId,
                UserId = user.Id,
                Role = parsedRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        var effectiveRole = await ResolveAccountRoleAsync(user, accountId);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, accountId, effectiveRole);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} registered successfully", email);

        var userInfo = new AuthUserInfo(user.Id, user.Name, user.Email ?? email, user.Avatar, effectiveRole, accountId, user.AvailabilityStatus.ToString());
        return new AuthResult(true, accessToken, refreshToken, User: userInfo);
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

        // Check if MFA is enabled
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogInformation("MFA required for user {Email}", email);
            return new AuthResult(false, null, null, new[] { "MFA_REQUIRED" },
                new AuthUserInfo(user.Id, user.Name, user.Email ?? email, user.Avatar, "", accountId, ""));
        }

        var role = await ResolveAccountRoleAsync(user, accountId);

        var accessToken = _jwtTokenService.GenerateAccessToken(user, accountId, role);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} logged in successfully with role {Role} for account {AccountId}", email, role, accountId);

        var userInfo = new AuthUserInfo(user.Id, user.Name, user.Email ?? email, user.Avatar, role, accountId, user.AvailabilityStatus.ToString());
        return new AuthResult(true, accessToken, refreshToken, User: userInfo);
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

        var role = await ResolveAccountRoleAsync(user, accountId);

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

    public async Task<AuthResult> GenerateTokensForUserAsync(int userId, int accountId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return new AuthResult(false, null, null, new[] { "User not found." });

        var role = await ResolveAccountRoleAsync(user, accountId);

        var accessToken = _jwtTokenService.GenerateAccessToken(user, accountId, role);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var userInfo = new AuthUserInfo(user.Id, user.Name, user.Email ?? "", user.Avatar, role, accountId, user.AvailabilityStatus.ToString());
        return new AuthResult(true, accessToken, refreshToken, User: userInfo);
    }

    public async Task RevokeTokenAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            // Rotating the security stamp invalidates all active sessions for this user.
            // Any subsequent token refresh will fail because the stamp no longer matches.
            await _userManager.UpdateSecurityStampAsync(user);
            _logger.LogInformation("Security stamp rotated for user {UserId} on logout", userId);
        }
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Email confirmation failed: user {Email} not found", email);
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed for {Email}", email);
            return true;
        }

        _logger.LogWarning("Email confirmation failed for {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
        return false;
    }
}
