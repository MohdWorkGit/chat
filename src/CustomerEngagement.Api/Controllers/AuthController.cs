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
        return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromBody] Application.Auth.Commands.LoginCommand command)
    {
        var result = await _mediator.Send(command);
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

    [HttpPost("forgot_password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] Application.Auth.Commands.ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset_password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(
        [FromBody] Application.Auth.Commands.ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Password has been reset successfully." });
    }

    [HttpPost("confirm_email")]
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
}
