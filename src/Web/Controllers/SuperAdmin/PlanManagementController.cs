using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Core.PlanManagement.Commands;
using HotelManagement.Application.Core.PlanManagement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin plan management controller
/// </summary>
[ApiController]
[Route("cp/plans")]
[Authorize(Roles = "SuperAdmin")]
public class PlanManagementController(
    ISuperAdminService superAdminService,
    ISender mediator,
    ILogger<PlanManagementController> logger) : ControllerBase
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly ILogger<PlanManagementController> _logger = logger;
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Get list of plans with filtering and pagination
    /// </summary>
    /// <param name="request">List request parameters</param>
    /// <returns>Paginated list of plans</returns>
    [HttpGet]
    public async Task<ActionResult> GetPlans([FromQuery] GetPlansQuery request)
    {
        var response = await _mediator.Send(request);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Create a new subscription plan
    /// </summary>
    /// <param name="command">Plan creation request</param>
    /// <returns>Created plan result</returns>
    [HttpPost]
    public async Task<ActionResult> CreatePlan([FromBody] CreatePlanCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get a specific plan by ID
    /// </summary>
    /// <param name="planId">Plan ID</param>
    /// <returns>Plan details</returns>
    [HttpGet("{planId}")]
    public async Task<ActionResult> GetPlanById(Guid planId)
    {
        var query = new GetPlanByIdQuery { PlanId = planId };
        var response = await _mediator.Send(query);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Update an existing plan
    /// </summary>
    /// <param name="planId">Plan ID</param>
    /// <param name="command">Plan update request</param>
    /// <returns>Updated plan result</returns>
    [HttpPatch("{planId}")]
    public async Task<ActionResult> UpdatePlan(Guid planId, [FromBody] UpdatePlanCommand command)
    {
        var updateCommand = command with { PlanId = planId };
        var response = await _mediator.Send(updateCommand);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Delete a plan
    /// </summary>
    /// <param name="planId">Plan ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{planId}")]
    public async Task<ActionResult> DeletePlan(Guid planId)
    {
        var command = new DeletePlanCommand { PlanId = planId };
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Assign a module to a plan
    /// </summary>
    /// <param name="planId">Plan ID</param>
    /// <param name="moduleId">Module ID</param>
    /// <returns>Assignment result</returns>
    [HttpPost("{planId}/modules/{moduleId}")]
    public async Task<ActionResult> AssignModuleToPlan(Guid planId, Guid moduleId)
    {
        var command = new AssignModuleToPlanCommand { PlanId = planId, ModuleId = moduleId };
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Remove a module from a plan
    /// </summary>
    /// <param name="planId">Plan ID</param>
    /// <param name="moduleId">Module ID</param>
    /// <returns>Removal result</returns>
    [HttpDelete("{planId}/modules/{moduleId}")]
    public async Task<ActionResult> RemoveModuleFromPlan(Guid planId, Guid moduleId)
    {
        var command = new RemoveModuleFromPlanCommand { PlanId = planId, ModuleId = moduleId };
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Bulk update multiple plans
    /// </summary>
    /// <param name="command">Bulk update request</param>
    /// <returns>Bulk update result</returns>
    [HttpPatch("bulk")]
    public async Task<ActionResult> BulkUpdatePlans([FromBody] BulkUpdatePlansCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get plan usage analytics
    /// </summary>
    /// <returns>Plan usage analytics</returns>
    [HttpGet("usage")]
    public async Task<ActionResult> GetPlanUsage()
    {
        var query = new GetPlanUsageQuery();
        var response = await _mediator.Send(query);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }
}
