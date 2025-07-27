using HotelManagement.Application.Common.Security;
using HotelManagement.Application.Core.Role.Commands;
using HotelManagement.Application.Core.Role.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Administrator;

[ApiController]
[Route("api/v1/role")]
[Authorize(Roles = "Administrator")]
public class RoleController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleCommand command)
    {
        if (command == null)
            return BadRequest("Invalid role data.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Route("assign")]
    public async Task<IActionResult> AssignRoleToUserAsync([FromBody] AssignRoleToUserCommand command)
    {
        if (command == null)
            return BadRequest("Invalid role assignment data.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAllRolesAsync()
    {
        var result = await _mediator.Send(new GetAllRolesQuery());
        return Ok(result);
    }

    [HttpGet]
    [Route("user/{id}")]
    public async Task<IActionResult> GetUserRolesAsync(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid user ID.");

        var result = await _mediator.Send(new GetUserRolesQuery { UserId = id });
        return Ok(result);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetRoleByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid role ID.");

        var result = await _mediator.Send(new GetRoleByIdQuery { Id = id });
        return Ok(result);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateRoleAsync([FromBody] UpdateRoleCommand command)
    {
        if (command == null)
            return BadRequest("Invalid role data.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteRoleAsync(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid role ID.");

        var result = await _mediator.Send(new DeleteRoleCommand { Id = id });
        return Ok(result);
    }
}
