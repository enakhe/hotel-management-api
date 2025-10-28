using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers;

/// <summary>
/// Controller for tenant settings and feature access
/// </summary>
[ApiController]
[Route("api/v1/tenant-settings")]
[Authorize]
public class TenantSettingsController(
    ILicensingService licensingService,
    ITenantContext tenantContext,
    ILogger<TenantSettingsController> logger) : ControllerBase
{
    private readonly ILicensingService _licensingService = licensingService;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly ILogger<TenantSettingsController> _logger = logger;

    /// <summary>
    /// Gets tenant settings including enabled features
    /// </summary>
    /// <returns>Tenant settings</returns>
    [HttpGet("settings")]
    public async Task<ActionResult<TenantSettings>> GetTenantSettings()
    {
        if (!_tenantContext.IsResolved)
        {
            return BadRequest("Tenant context not resolved");
        }

        var settings = await _licensingService.GetTenantSettingsAsync(_tenantContext.TenantId!.Value);

        return settings == null ? (ActionResult<TenantSettings>)NotFound("Tenant not found") : (ActionResult<TenantSettings>)Ok(settings);
    }

    /// <summary>
    /// Checks if a specific feature is enabled for the current tenant
    /// </summary>
    /// <param name="featureName">The feature name to check</param>
    /// <returns>True if the feature is enabled</returns>
    [HttpGet("features/{featureName}/enabled")]
    public async Task<ActionResult<bool>> IsFeatureEnabled(string featureName)
    {
        if (!_tenantContext.IsResolved)
        {
            return BadRequest("Tenant context not resolved");
        }

        var isEnabled = await _licensingService.IsFeatureAllowedAsync(_tenantContext.TenantId!.Value, featureName);
        return Ok(isEnabled);
    }

    /// <summary>
    /// Gets all enabled features for the current tenant
    /// </summary>
    /// <returns>List of enabled feature names</returns>
    [HttpGet("features")]
    public async Task<ActionResult<IEnumerable<string>>> GetEnabledFeatures()
    {
        if (!_tenantContext.IsResolved)
        {
            return BadRequest("Tenant context not resolved");
        }

        var features = await _licensingService.GetEnabledFeaturesAsync(_tenantContext.TenantId!.Value);
        return Ok(features);
    }

    /// <summary>
    /// Checks if the current tenant's license is valid
    /// </summary>
    /// <returns>True if the license is valid</returns>
    [HttpGet("license/valid")]
    public async Task<ActionResult<bool>> IsLicenseValid()
    {
        if (!_tenantContext.IsResolved)
        {
            return BadRequest("Tenant context not resolved");
        }

        var isValid = await _licensingService.IsLicenseValidAsync(_tenantContext.TenantId!.Value);
        return Ok(isValid);
    }
}
