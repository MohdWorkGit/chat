using MediatR;

namespace CustomerEngagement.Application.Auth.Commands;

public record SetupMfaCommand(int UserId) : IRequest<MfaSetupResult>;

public class SetupMfaCommandHandler : IRequestHandler<SetupMfaCommand, MfaSetupResult>
{
    private readonly IMfaService _mfaService;

    public SetupMfaCommandHandler(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    public async Task<MfaSetupResult> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        return await _mfaService.SetupMfaAsync(request.UserId);
    }
}

public record EnableMfaCommand(int UserId, string OtpCode) : IRequest<(bool Succeeded, string[]? BackupCodes, IEnumerable<string>? Errors)>;

public class EnableMfaCommandHandler : IRequestHandler<EnableMfaCommand, (bool Succeeded, string[]? BackupCodes, IEnumerable<string>? Errors)>
{
    private readonly IMfaService _mfaService;

    public EnableMfaCommandHandler(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    public async Task<(bool Succeeded, string[]? BackupCodes, IEnumerable<string>? Errors)> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        return await _mfaService.EnableMfaAsync(request.UserId, request.OtpCode);
    }
}

public record DisableMfaCommand(int UserId, string OtpCode) : IRequest<(bool Succeeded, IEnumerable<string>? Errors)>;

public class DisableMfaCommandHandler : IRequestHandler<DisableMfaCommand, (bool Succeeded, IEnumerable<string>? Errors)>
{
    private readonly IMfaService _mfaService;

    public DisableMfaCommandHandler(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        return await _mfaService.DisableMfaAsync(request.UserId, request.OtpCode);
    }
}

public record VerifyMfaCommand(int UserId, string OtpCode, int AccountId) : IRequest<MfaVerifyResult>;

public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, MfaVerifyResult>
{
    private readonly IMfaService _mfaService;
    private readonly IIdentityService _identityService;

    public VerifyMfaCommandHandler(IMfaService mfaService, IIdentityService identityService)
    {
        _mfaService = mfaService;
        _identityService = identityService;
    }

    public async Task<MfaVerifyResult> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _mfaService.ValidateOtpAsync(request.UserId, request.OtpCode);
        if (!isValid)
        {
            // Try backup code
            isValid = await _mfaService.ValidateBackupCodeAsync(request.UserId, request.OtpCode);
        }

        if (!isValid)
            return new MfaVerifyResult(false, null, null, new[] { "Invalid OTP code." });

        // Generate tokens after successful MFA verification
        var result = await _identityService.GenerateTokensForUserAsync(request.UserId, request.AccountId);
        return new MfaVerifyResult(result.Succeeded, result.AccessToken, result.RefreshToken, result.Errors);
    }
}

public record RegenerateBackupCodesCommand(int UserId) : IRequest<string[]>;

public class RegenerateBackupCodesCommandHandler : IRequestHandler<RegenerateBackupCodesCommand, string[]>
{
    private readonly IMfaService _mfaService;

    public RegenerateBackupCodesCommandHandler(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    public async Task<string[]> Handle(RegenerateBackupCodesCommand request, CancellationToken cancellationToken)
    {
        return await _mfaService.RegenerateBackupCodesAsync(request.UserId);
    }
}
