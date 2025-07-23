using HotelManagement.Application.Core.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        if (command == null)
            return BadRequest("Request payload is missing or malformed.");

        var userId = await _mediator.Send(command);

        return CreatedAtAction(nameof(Register), new { id = userId }, new { id = userId });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.RefreshToken))
            return BadRequest("Refresh token is required.");

        var tokenResponse = await _mediator.Send(command);

        return tokenResponse == null ? Unauthorized("Invalid refresh token.") : Ok(tokenResponse);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.NewPassword) || string.IsNullOrWhiteSpace(command.CurrentPassword))
            return BadRequest("Old and new passwords are required.");

        await _mediator.Send(command);

        return Ok(new { Message = "Password changed successfully." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] RequestPasswordResetCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.Email))
            return BadRequest("Email is required.");

        await _mediator.Send(command);
        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("confirm-password-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Token) || string.IsNullOrWhiteSpace(command.Password))
            return BadRequest("Email, token, and new password are required.");

        await _mediator.Send(command);
        return Ok(new { Message = "Password reset successfully." });
    }
}
