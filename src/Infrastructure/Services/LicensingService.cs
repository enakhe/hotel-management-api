using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Services.LicenseKey;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing tenant licensing and feature entitlements
/// </summary>
public class LicensingService : ILicensingService, ILicenseKeyService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LicensingService> _logger;
    private readonly ICacheService _cache;

    public LicensingService(ApplicationDbContext context, ILogger<LicensingService> logger, ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> IsLicenseValidAsync(Guid tenantId)
    {
        try
        {
            // Try to get from cache (15 minutes for validation data)
            var cacheKey = CacheKeys.LicenseValidation(tenantId);
            var cached = await _cache.GetAsync<bool?>(cacheKey);
            
            if (cached.HasValue)
            {
                _logger.LogDebug("Returning cached license validation for tenant: {TenantId}", tenantId);
                return cached.Value;
            }

            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.License)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            var isValid = tenant != null
                && tenant.IsActive
                && (tenant.License!.ExpirationDate >= DateTime.UtcNow);

            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, isValid, TimeSpan.FromMinutes(15));

            return isValid;
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
            // Try to get from cache (15 minutes for feature validation)
            var cacheKey = $"tenant:{tenantId}:feature:{featureName}:allowed";
            var cached = await _cache.GetAsync<bool?>(cacheKey);
            
            if (cached.HasValue)
            {
                _logger.LogDebug("Returning cached feature validation for tenant: {TenantId}, feature: {Feature}", tenantId, featureName);
                return cached.Value;
            }

            if (!await IsLicenseValidAsync(tenantId))
                return false;

            // Get tenant with plan and modules to check feature access
            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.License)
                .Include(t => t.License!.Plan)
                .Include(t => t.License!.Plan!.PlanModules)
                    .ThenInclude(pm => pm.Module!.Features)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant?.License?.Plan == null)
                return false;

            // Check if the feature is available in any of the plan's modules
            var hasFeature = tenant.License.Plan.PlanModules
                .Any(pm => pm.Module!.Features.Any(f => f.Name == featureName && f.IsEnabled));

            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, hasFeature, TimeSpan.FromMinutes(15));

            return hasFeature;
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
            // Get tenant with plan and modules to get enabled features
            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.License)
                .Include(t => t.License!.Plan)
                .Include(t => t.License!.Plan!.PlanModules)
                    .ThenInclude(pm => pm.Module!.Features)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant?.License?.Plan == null)
                return [];

            // Get all enabled features from the plan's modules
            var enabledFeatures = tenant.License.Plan.PlanModules
                .SelectMany(pm => pm.Module!.Features.Where(f => f.IsEnabled))
                .Select(f => f.Name)
                .ToList();

            return enabledFeatures;
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
                .Include(t => t.License)
                .Include(t => t.License!.Plan)
                .Include(t => t.License!.Plan!.PlanModules)
                    .ThenInclude(pm => pm.Module!.Features)
                .Include(t => t.License!.Plan!.Limits)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null || tenant.License == null || tenant.License.Plan == null)
                return null;

            // Get enabled features from plan modules
            var enabledFeatures = tenant.License.Plan.PlanModules
                .SelectMany(pm => pm.Module!.Features.Where(f => f.IsEnabled))
                .Select(f => f.Name)
                .ToList();

            return new TenantSettings
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                TenantIdentifier = tenant.Identifier,
                IsActive = tenant.IsActive,
                SubscriptionPlan = tenant.License.Plan.Name ?? "Basic",
                SubscriptionEndDate = tenant.License.ExpirationDate,
                MaxUsers = tenant.License.Plan.Limits?.MaxUsers ?? 0,
                MaxBranches = tenant.License.Plan.Limits?.MaxBranches ?? 0,
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

    public Task<Result<string>> GenerateLicenseKeyAsync(Application.Common.Services.LicenseKey.LicenseKeyGenerationOptions options)
    {
        try
        {
            var key = LicenseKeyGeneratorService.Generate(options);
            return Task.FromResult(Result<string>.Success(key, 200));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid license key generation options");
            return Task.FromResult(Result<string>.Failure(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating license key");
            return Task.FromResult(Result<string>.Failure("An error occurred while generating the license key", 500));
        }
    }

    public Task<Result<LicenseKeyValidationResult>> ValidateLicenseKeyFormatAsync(string key, LicenseKeyFormat format, bool includeChecksum = true)
    {
        try
        {
            var result = LicenseKeyGeneratorService.ValidateDetailed(key, format, includeChecksum);
            return Task.FromResult(Result<LicenseKeyValidationResult>.Success(result, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license key format for key: {Key}", key);
            return Task.FromResult(Result<LicenseKeyValidationResult>.Failure("An error occurred while validating the license key", 500));
        }
    }

    public async Task<Result<bool>> IsLicenseKeyUniqueAsync(string key)
    {
        try
        {
            var existingLicense = await _context.Licenses
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LicenseKey == key);

            return Result<bool>.Success(existingLicense == null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking license key uniqueness for key: {Key}", key);
            return Result<bool>.Failure("An error occurred while checking license key uniqueness", 500);
        }
    }

    public Task<Result<string>> MaskLicenseKeyForDisplayAsync(string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return Task.FromResult(Result<string>.Failure("License key cannot be null or empty", 400));

            // Mask all characters except the first 4 and last 4
            if (key.Length <= 8)
                return Task.FromResult(Result<string>.Success(new string('*', key.Length), 200));

            var maskedKey = key.Substring(0, 4) + new string('*', key.Length - 8) + key.Substring(key.Length - 4);
            return Task.FromResult(Result<string>.Success(maskedKey, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error masking license key for display");
            return Task.FromResult(Result<string>.Failure("An error occurred while masking the license key", 500));
        }
    }

    public Task<Result<string>> FormatLicenseKeyForDisplayAsync(string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return Task.FromResult(Result<string>.Failure("License key cannot be null or empty", 400));

            // Remove any existing separators and format with dashes
            var cleanKey = key.Replace("-", "").Replace("_", "").Replace(" ", "");

            // Format as XXXX-XXXX-XXXX-XXXX
            var formattedKey = string.Join("-",
                Enumerable.Range(0, cleanKey.Length / 4)
                    .Select(i => cleanKey.Substring(i * 4, Math.Min(4, cleanKey.Length - i * 4))));

            return Task.FromResult(Result<string>.Success(formattedKey, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting license key for display");
            return Task.FromResult(Result<string>.Failure("An error occurred while formatting the license key", 500));
        }
    }
}
