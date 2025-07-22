using HotelManagement.Application.Auth.Commands;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Security;
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

        if (tokenResponse == null)
            return Unauthorized("Invalid refresh token.");

        return Ok(tokenResponse);
    }
}
