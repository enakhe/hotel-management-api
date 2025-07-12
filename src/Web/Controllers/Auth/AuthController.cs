using HotelManagement.Application.Auth.Commands;
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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        var response = await _mediator.Send(new LoginCommand(loginRequest));
        return Ok(response);
    }
}
