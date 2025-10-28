using HotelManagement.Application.Core.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Auth;

/// <summary>
///    Authentication and Authorization Controller
/// </summary>
/// <param name="mediator"></param>

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] RequestPasswordResetCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    [HttpPost("confirm-password-reset")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }
}
