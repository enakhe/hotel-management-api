using HotelManagement.Application.Core.TenantAdminManagement.Commands;
using HotelManagement.Application.Core.TenantAdminManagement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin controller for managing Tenant Administrator accounts
/// </summary>
[ApiController]
[Route("cp/tenant-admins")]
[Authorize(Roles = "SuperAdmin")]
public class TenantAdminController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<TenantAdminController> _logger;

    public TenantAdminController(ISender mediator, ILogger<TenantAdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Tenant Administrator
    /// </summary>
    /// <param name="command">Tenant admin creation details</param>
    /// <returns>Created tenant admin details</returns>
    /// <response code="201">Tenant admin created successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Tenant not found</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTenantAdmin([FromBody] CreateTenantAdminCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(
            nameof(GetTenantAdminById),
            new { id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Get all Tenant Administrators with pagination and filtering
    /// </summary>
    /// <param name="query">Filter and pagination parameters</param>
    /// <returns>Paginated list of tenant admins</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenantAdmins([FromQuery] GetTenantAdminsQuery query)
    {
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Get a Tenant Administrator by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Tenant admin details</returns>
    /// <response code="200">Tenant admin found</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantAdminById(Guid id)
    {
        var query = new GetTenantAdminByIdQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Get the Administrator for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Tenant admin details</returns>
    /// <response code="200">Tenant admin found</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantAdminByTenantId(Guid tenantId)
    {
        var query = new GetTenantAdminByTenantIdQuery { TenantId = tenantId };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Update a Tenant Administrator's profile
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Update details</param>
    /// <returns>Updated tenant admin details</returns>
    /// <response code="200">Tenant admin updated successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenantAdmin(Guid id, [FromBody] UpdateTenantAdminCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID in URL does not match ID in request body" });

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Reset a Tenant Administrator's password
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Password reset details</param>
    /// <returns>Success result</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpPatch("{id}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetTenantAdminPasswordCommand command)
    {
        if (id != command.UserId)
            return BadRequest(new { Message = "ID in URL does not match ID in request body" });

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(new { Message = "Password reset successfully" });
    }

    /// <summary>
    /// Activate a Tenant Administrator account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success result</returns>
    /// <response code="200">Tenant admin activated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateTenantAdmin(Guid id)
    {
        var command = new ActivateTenantAdminCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(new { Message = "Tenant admin activated successfully" });
    }

    /// <summary>
    /// Deactivate a Tenant Administrator account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success result</returns>
    /// <response code="200">Tenant admin deactivated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTenantAdmin(Guid id)
    {
        var command = new DeactivateTenantAdminCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(new { Message = "Tenant admin deactivated successfully" });
    }

    /// <summary>
    /// Delete a Tenant Administrator
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success result</returns>
    /// <response code="200">Tenant admin deleted successfully</response>
    /// <response code="400">Cannot delete (validation error)</response>
    /// <response code="404">Tenant admin not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenantAdmin(Guid id)
    {
        var command = new DeleteTenantAdminCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        return Ok(new { Message = "Tenant admin deleted successfully" });
    }
}

