using HotelManagement.Application.Common.DTOs.License;
using HotelManagement.Application.Common.Interfaces.License;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing tenant licensing and feature entitlements
/// </summary>
public class LicensingService : ILicensingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LicensingService> _logger;

    public LicensingService(ApplicationDbContext context, ILogger<LicensingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsLicenseValidAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            return tenant != null 
                && tenant.IsActive 
                && (!tenant.SubscriptionEndDate.HasValue || tenant.SubscriptionEndDate.Value >= DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> IsFeatureAllowedAsync(Guid tenantId, string featureName)
    {
        try
        {
            if (!await IsLicenseValidAsync(tenantId))
                return false;

            var feature = await _context.TenantFeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.FeatureName == featureName);

            return feature?.IsEnabled == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature access for tenant {TenantId}, feature {FeatureName}", tenantId, featureName);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetEnabledFeaturesAsync(Guid tenantId)
    {
        try
        {
            var features = await _context.TenantFeatures
                .AsNoTracking()
                .Where(f => f.TenantId == tenantId && f.IsEnabled)
                .Select(f => f.FeatureName)
                .ToListAsync();

            return features;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enabled features for tenant {TenantId}", tenantId);
            return [];
        }
    }

    public async Task<TenantSettings?> GetTenantSettingsAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.Features.Where(f => f.IsEnabled))
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return null;

            var enabledFeatures = tenant.Features.Select(f => f.FeatureName).ToList();

            return new TenantSettings
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                TenantIdentifier = tenant.Identifier,
                IsActive = tenant.IsActive,
                SubscriptionPlan = tenant.SubscriptionPlan ?? "Basic",
                SubscriptionEndDate = tenant.SubscriptionEndDate,
                MaxUsers = tenant.MaxUsers,
                MaxBranches = tenant.MaxBranches,
                EnabledFeatures = enabledFeatures,
                CustomSettings = new Dictionary<string, object>
                {
                    ["timeZone"] = tenant.TimeZone ?? "WAT",
                    ["currencyCode"] = tenant.CurrencyCode ?? "NGN",
                    ["languageCode"] = tenant.LanguageCode ?? "en"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant settings for tenant {TenantId}", tenantId);
            return null;
        }
    }
}
