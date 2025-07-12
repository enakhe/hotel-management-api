using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Security;
using HotelManagement.Application.Users.Commands;
using HotelManagement.Application.Users.Queries;
using MediatR;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Administrator;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public class UserController(ISender mediator, IUserService userService) : ControllerBase
{
    private readonly ISender _mediator = mediator;
    private readonly IUserService _userService = userService;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var id = await _mediator.Send(new CreateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { id }, new { id });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        await _mediator.Send(new UpdateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { dto.Id }, new { dto.Id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery { Id = id });
        return Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand { Id = id });
        return Ok(new 
        { 
            Message = "User deleted successfully." 
        });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await _mediator.Send(new DeactivateUserCommand { Id = id});
        return Ok(new
        {
            Message = "User deactivated successfully."
        });
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await _mediator.Send(new ActivateUserCommand { Id = id });
        return Ok(new
        {
            Message = "User activated successfully."
        });
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, List<Guid> roleIds)
    {
        await _mediator.Send(new AssignRolesCommand { Id = id, RoleIds = roleIds});
        return Ok(new
        {
            Message = "Roles assigned successfully."
        });
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<string>>> GetUserRoles(Guid id)
    {
        var roles = await _mediator.Send(new GetUserRolesQuery { Id = id });
        return Ok(roles);
    }
}
