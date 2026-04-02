using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CustomerEngagement.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(
        [FromBody] Application.Auth.Commands.RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors ?? []) });
        return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromBody] Application.Auth.Commands.LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return Unauthorized(new { message = string.Join("; ", result.Errors ?? []) });
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        await _mediator.Send(new Application.Auth.Commands.LogoutCommand(userId, token));
        return Ok();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken(
        [FromBody] Application.Auth.Commands.RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] Application.Auth.Commands.ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(
        [FromBody] Application.Auth.Commands.ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Password has been reset successfully." });
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(
        [FromBody] Application.Auth.Commands.ConfirmEmailCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Email confirmed successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(new Application.Auth.Queries.GetCurrentUserQuery(userId));
        return Ok(result);
    }

    // --- MFA/2FA Endpoints ---

    [HttpPost("mfa/setup")]
    [Authorize]
    public async Task<ActionResult> SetupMfa()
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(new Application.Auth.Commands.SetupMfaCommand(userId));
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors ?? []) });
        return Ok(new { result.SecretKey, result.QrCodeUri });
    }

    [HttpPost("mfa/enable")]
    [Authorize]
    public async Task<ActionResult> EnableMfa([FromBody] MfaCodeRequest request)
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(new Application.Auth.Commands.EnableMfaCommand(userId, request.OtpCode));
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors ?? []) });
        return Ok(new { result.BackupCodes });
    }

    [HttpPost("mfa/disable")]
    [Authorize]
    public async Task<ActionResult> DisableMfa([FromBody] MfaCodeRequest request)
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(new Application.Auth.Commands.DisableMfaCommand(userId, request.OtpCode));
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors ?? []) });
        return Ok(new { message = "MFA disabled successfully." });
    }

    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        var result = await _mediator.Send(
            new Application.Auth.Commands.VerifyMfaCommand(request.UserId, request.OtpCode, request.AccountId));
        if (!result.Succeeded)
            return Unauthorized(new { message = string.Join("; ", result.Errors ?? []) });
        return Ok(new { result.AccessToken, result.RefreshToken });
    }

    [HttpPost("mfa/backup-codes/regenerate")]
    [Authorize]
    public async Task<ActionResult> RegenerateBackupCodes()
    {
        var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
        var codes = await _mediator.Send(new Application.Auth.Commands.RegenerateBackupCodesCommand(userId));
        return Ok(new { BackupCodes = codes });
    }
}

public record MfaCodeRequest(string OtpCode);
public record MfaVerifyRequest(int UserId, string OtpCode, int AccountId = 1);
