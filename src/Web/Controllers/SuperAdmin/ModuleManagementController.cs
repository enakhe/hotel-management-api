using HotelManagement.Application.Core.ModuleManagement.Commands;
using HotelManagement.Application.Core.ModuleManagement.Queries;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin module management controller
/// </summary>
[ApiController]
[Route("cp/modules")]
[Authorize(Roles = "SuperAdmin")]
public class ModuleManagementController(
    ISuperAdminService superAdminService,
    ISender mediator,
    ILogger<ModuleManagementController> logger) : ControllerBase
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly ILogger<ModuleManagementController> _logger = logger;
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Get list of modules with filtering and pagination
    /// </summary>
    /// <param name="request">List request parameters</param>
    /// <returns>Paginated list of modules</returns>
    [HttpGet]
    public async Task<ActionResult> GetModules([FromQuery] GetModulesQuery request)
    {
        var response = await _mediator.Send(request);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Create a new module
    /// </summary>
    /// <param name="command">Module creation request</param>
    /// <returns>Created module result</returns>
    [HttpPost]
    public async Task<ActionResult> CreateModule([FromBody] CreateModuleCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get a specific module by ID
    /// </summary>
    /// <param name="moduleId">Module ID</param>
    /// <returns>Module details</returns>
    [HttpGet("{moduleId}")]
    public async Task<ActionResult> GetModuleById(Guid moduleId)
    {
        var query = new GetModuleByIdQuery { ModuleId = moduleId };
        var response = await _mediator.Send(query);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Update an existing module
    /// </summary>
    /// <param name="moduleId">Module ID</param>
    /// <param name="command">Module update request</param>
    /// <returns>Updated module result</returns>
    [HttpPatch("{moduleId}")]
    public async Task<ActionResult> UpdateModule(Guid moduleId, [FromBody] UpdateModuleCommand command)
    {
        var updateCommand = command with { ModuleId = moduleId };
        var response = await _mediator.Send(updateCommand);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Delete a module
    /// </summary>
    /// <param name="moduleId">Module ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{moduleId}")]
    public async Task<ActionResult> DeleteModule(Guid moduleId)
    {
        var command = new DeleteModuleCommand { ModuleId = moduleId };
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Bulk update multiple modules
    /// </summary>
    /// <param name="command">Bulk update request</param>
    /// <returns>Bulk update result</returns>
    [HttpPatch("bulk")]
    public async Task<ActionResult> BulkUpdateModules([FromBody] BulkUpdateModulesCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }
}
