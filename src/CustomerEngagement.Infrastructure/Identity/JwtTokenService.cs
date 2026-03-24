using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CustomerEngagement.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CustomerEngagement.Infrastructure.Identity;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetSecretKey() =>
        _configuration["JWT_SECRET"]
        ?? _configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT secret key is not configured.");

    public string GenerateAccessToken(User user, int accountId, string role)
    {
        var secretKey = GetSecretKey();
        var issuer = _configuration["Jwt:Issuer"] ?? "CustomerEngagement";
        var audience = _configuration["Jwt:Audience"] ?? "CustomerEngagement";
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("uid", user.Id.ToString()),
            new("UserId", user.Id.ToString()),
            new("account_id", accountId.ToString()),
            new("AccountId", accountId.ToString()),
            new(ClaimTypes.Role, role),
            new("Name", user.Name ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        _logger.LogDebug("Generated JWT access token for user {UserId} in account {AccountId}", user.Id, accountId);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secretKey = GetSecretKey();
        var issuer = _configuration["Jwt:Issuer"] ?? "CustomerEngagement";
        var audience = _configuration["Jwt:Audience"] ?? "CustomerEngagement";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        var userIdClaim = principal?.FindFirst("UserId")?.Value;
        return userIdClaim is not null ? int.Parse(userIdClaim) : null;
    }
}
