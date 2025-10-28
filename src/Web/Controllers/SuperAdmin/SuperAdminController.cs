using HotelManagement.Application.Core.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin main controller
/// </summary>
[ApiController]
[Route("cp")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController(ISender mediator, ILogger<SuperAdminController> logger) : ControllerBase
{
    private readonly ISender _mediator = mediator;
    private readonly ILogger<SuperAdminController> _logger = logger;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginCommand command)
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
}
