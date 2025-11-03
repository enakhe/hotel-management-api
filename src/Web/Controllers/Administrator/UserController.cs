using Asp.Versioning;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Security;
using HotelManagement.Application.Core.Users.Commands;
using HotelManagement.Application.Core.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Administrator;

/// <summary>
/// User management controller for tenant administrators
/// </summary>
/// <remarks>
/// Provides endpoints for creating, reading, updating, and deleting users within a tenant.
/// All operations are tenant-scoped and require Administrator role.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class UserController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Creates a new user in the current tenant
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <returns>The ID of the newly created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="409">Conflict - user already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var id = await _mediator.Send(new CreateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { id }, new { id });
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="dto">User update data</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        await _mediator.Send(new UpdateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { dto.Id }, new { dto.Id });
    }

    /// <summary>
    /// Retrieves a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">User found</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery { Id = id });
        return Ok(user);
    }

    /// <summary>
    /// Retrieves all users in the current tenant
    /// </summary>
    /// <returns>List of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">User ID to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="404">User not found</response>
    /// <remarks>
    /// This is a soft delete operation. The user record is marked as deleted but not removed from the database.
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand { Id = id });
        return Ok(new
        {
            Message = "User deleted successfully."
        });
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">User deactivated successfully</response>
    /// <response code="404">User not found</response>
    /// <remarks>
    /// Deactivated users cannot login but their data is preserved.
    /// Use this instead of delete for temporary account suspension.
    /// </remarks>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await _mediator.Send(new DeactivateUserCommand { Id = id });
        return Ok(new
        {
            Message = "User deactivated successfully."
        });
    }

    /// <summary>
    /// Activates a previously deactivated user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">User activated successfully</response>
    /// <response code="404">User not found</response>
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await _mediator.Send(new ActivateUserCommand { Id = id });
        return Ok(new
        {
            Message = "User activated successfully."
        });
    }

    /// <summary>
    /// Assigns roles to a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="roleIds">List of role IDs to assign</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Roles assigned successfully</response>
    /// <response code="404">User or role not found</response>
    /// <remarks>
    /// This operation replaces all existing roles with the provided list.
    /// To add a single role, include all existing roles plus the new one.
    /// </remarks>
    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] List<Guid> roleIds)
    {
        await _mediator.Send(new AssignRolesCommand { Id = id, RoleIds = roleIds });
        return Ok(new
        {
            Message = "Roles assigned successfully."
        });
    }

    /// <summary>
    /// Retrieves all roles assigned to a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>List of role names</returns>
    /// <response code="200">Roles retrieved successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}/roles")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> GetUserRoles(Guid id)
    {
        var roles = await _mediator.Send(new GetUserRolesQuery { Id = id });
        return Ok(roles);
    }
}

