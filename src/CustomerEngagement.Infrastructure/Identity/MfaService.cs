using System.Security.Cryptography;
using CustomerEngagement.Application.Auth;
using CustomerEngagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.Identity;

public class MfaService : IMfaService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<MfaService> _logger;
    private const string Issuer = "CustomerEngagement";
    private const int BackupCodeCount = 10;

    public MfaService(UserManager<User> userManager, ILogger<MfaService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<MfaSetupResult> SetupMfaAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return new MfaSetupResult(false, null, null, null, new[] { "User not found." });

        if (await _userManager.GetTwoFactorEnabledAsync(user))
            return new MfaSetupResult(false, null, null, null, new[] { "MFA is already enabled." });

        // Reset the authenticator key to generate a new one
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var secretKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(secretKey))
            return new MfaSetupResult(false, null, null, null, new[] { "Failed to generate secret key." });

        var email = user.Email ?? user.UserName ?? "user";
        var qrCodeUri = $"otpauth://totp/{Issuer}:{email}?secret={secretKey}&issuer={Issuer}&digits=6";

        _logger.LogInformation("MFA setup initiated for user {UserId}", userId);

        return new MfaSetupResult(true, secretKey, qrCodeUri, null);
    }

    public async Task<(bool Succeeded, string[]? BackupCodes, IEnumerable<string>? Errors)> EnableMfaAsync(int userId, string otpCode)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (false, null, new[] { "User not found." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, otpCode);

        if (!isValid)
            return (false, null, new[] { "Invalid verification code." });

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Generate backup codes
        var backupCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, BackupCodeCount);

        _logger.LogInformation("MFA enabled for user {UserId}", userId);

        return (true, backupCodes?.ToArray(), null);
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> DisableMfaAsync(int userId, string otpCode)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (false, new[] { "User not found." });

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            return (false, new[] { "MFA is not enabled." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, otpCode);

        if (!isValid)
            return (false, new[] { "Invalid verification code." });

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("MFA disabled for user {UserId}", userId);

        return (true, null);
    }

    public async Task<bool> ValidateOtpAsync(int userId, string otpCode)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        return await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, otpCode);
    }

    public async Task<bool> ValidateBackupCodeAsync(int userId, string backupCode)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, backupCode);
        if (result.Succeeded)
        {
            _logger.LogInformation("Backup code redeemed for user {UserId}", userId);
        }
        return result.Succeeded;
    }

    public async Task<bool> IsMfaEnabledAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        return await _userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<string[]> RegenerateBackupCodesAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            throw new InvalidOperationException("MFA is not enabled.");

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, BackupCodeCount);

        _logger.LogInformation("Backup codes regenerated for user {UserId}", userId);

        return codes?.ToArray() ?? [];
    }
}
