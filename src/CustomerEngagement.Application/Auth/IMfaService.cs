namespace CustomerEngagement.Application.Auth;

public record MfaSetupResult(bool Succeeded, string? SecretKey, string? QrCodeUri, string[]? BackupCodes, IEnumerable<string>? Errors = null);
public record MfaVerifyResult(bool Succeeded, string? AccessToken, string? RefreshToken, IEnumerable<string>? Errors = null);

public interface IMfaService
{
    Task<MfaSetupResult> SetupMfaAsync(int userId);
    Task<(bool Succeeded, string[]? BackupCodes, IEnumerable<string>? Errors)> EnableMfaAsync(int userId, string otpCode);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> DisableMfaAsync(int userId, string otpCode);
    Task<bool> ValidateOtpAsync(int userId, string otpCode);
    Task<bool> ValidateBackupCodeAsync(int userId, string backupCode);
    Task<bool> IsMfaEnabledAsync(int userId);
    Task<string[]> RegenerateBackupCodesAsync(int userId);
}
