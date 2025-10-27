using HotelManagement.Application.Common.DTOs.License;

namespace HotelManagement.Application.Common.Interfaces.License;

/// <summary>
/// Service for managing tenant licensing and feature entitlements
/// </summary>
public interface ILicensingService
{
    /// <summary>
    /// Checks if a tenant's license is valid (not expired, active, etc.)
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>True if the license is valid</returns>
    Task<bool> IsLicenseValidAsync(Guid tenantId);

    /// <summary>
    /// Checks if a tenant has access to a specific feature
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="featureName">The feature name (e.g., "PMS", "POS", "BAR")</param>
    /// <returns>True if the tenant has access to the feature</returns>
    Task<bool> IsFeatureAllowedAsync(Guid tenantId, string featureName);

    /// <summary>
    /// Gets all enabled features for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>List of enabled feature names</returns>
    Task<IEnumerable<string>> GetEnabledFeaturesAsync(Guid tenantId);

    /// <summary>
    /// Gets tenant settings including enabled features
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>Tenant settings</returns>
    Task<TenantSettings?> GetTenantSettingsAsync(Guid tenantId);
}
