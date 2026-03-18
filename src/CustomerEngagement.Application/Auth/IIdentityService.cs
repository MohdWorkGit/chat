namespace CustomerEngagement.Application.Auth;

public record AuthResult(bool Succeeded, string? AccessToken, string? RefreshToken, IEnumerable<string>? Errors = null);

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(string name, string email, string password, int accountId, string role = "Agent");
    Task<AuthResult> LoginAsync(string email, string password, int accountId);
    Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken, int accountId);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> ResetPasswordAsync(string email, string token, string newPassword);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
}
