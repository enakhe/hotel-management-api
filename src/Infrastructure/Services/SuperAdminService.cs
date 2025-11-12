using System.Text.Json;
using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for SuperAdmin tenant management operations
/// </summary>
public class SuperAdminService(
    ApplicationDbContext context,
    ILogger<SuperAdminService> logger,
    ISuperAdminAuditService auditService,
    IAuthService authService,
    IUserService userService,
    IMapper mapper,
    ITenantService tenantService,
    IPlanService planService,
    IModuleService moduleService,
    ILicenseService licenseService,
    ILicenseKeyService licenseKeyService,
    ILimitsService limitsService,
    ICacheService cache) : ISuperAdminService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<SuperAdminService> _logger = logger;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IPlanService _planService = planService;
    private readonly IModuleService _moduleService = moduleService;
    private readonly ILicenseService _licenseService = licenseService;
    private readonly ILicenseKeyService _licenseKeyService = licenseKeyService;
    private readonly ILimitsService _limitsService = limitsService;
    private readonly ICacheService _cache = cache;

    // Domain-specific services
    public ITenantService Tenants => _tenantService;
    public IPlanService Plans => _planService;
    public IModuleService Modules => _moduleService;
    public ILicenseService Licenses => _licenseService;
    public ILicenseKeyService LicenseKeys => _licenseKeyService;
    public ILimitsService Limits => _limitsService;

    // Cross-domain operations
    public async Task<Result<bool>> AssignPlanToTenantAsync(Guid tenantId, Guid planId, string reason)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return Result<bool>.Failure($"Tenant with ID '{tenantId}' not found", 404);

            // Validate plan exists
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
                return Result<bool>.Failure($"Plan with ID '{planId}' not found", 404);

            // Update tenant's plan
            tenant.PlanId = planId;
            tenant.LastModified = DateTime.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "AssignPlanToTenant",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Assigned plan '{plan.Name}' to tenant '{tenant.Name}'. Reason: {reason}",
                JsonSerializer.Serialize(new { PlanId = planId, Reason = reason }));

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning plan to tenant");
            return Result<bool>.Failure("An error occurred while assigning plan to tenant", 500);
        }
    }

    public async Task<Result<bool>> AssignLimitsToPlanAsync(Guid planId, Guid limitsId)
    {
        try
        {
            // Validate plan exists
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
                return Result<bool>.Failure($"Plan with ID '{planId}' not found", 404);

            // Validate limits exist
            var limits = await _context.Limits.FirstOrDefaultAsync(l => l.Id == limitsId);
            if (limits == null)
                return Result<bool>.Failure($"Limits with ID '{limitsId}' not found", 404);

            // Update plan's limits
            plan.LimitsId = limitsId;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "AssignLimitsToPlan",
                "Plan",
                planId.ToString(),
                planId,
                $"Assigned limits '{limits.Name}' to plan '{plan.Name}'",
                JsonSerializer.Serialize(new { LimitsId = limitsId }));

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning limits to plan");
            return Result<bool>.Failure("An error occurred while assigning limits to plan", 500);
        }
    }

    public async Task<Result<bool>> CreateTenantWithPlanAsync(CreateTenantWithPlanRequest request)
    {
        try
        {
            // Validate plan exists
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId);
            if (plan == null)
                return Result<bool>.Failure($"Plan with ID '{request.PlanId}' not found", 404);

            // Create tenant with the specified plan
            var tenantRequest = request.Tenant with { PlanId = request.PlanId };

            // If override limits specified, validate it exists
            if (request.OverrideLimitsId.HasValue)
            {
                var limits = await _context.Limits.FirstOrDefaultAsync(l => l.Id == request.OverrideLimitsId.Value);
                if (limits == null)
                    return Result<bool>.Failure($"Limits with ID '{request.OverrideLimitsId}' not found", 404);
            }

            var result = await _tenantService.CreateTenantAsync(tenantRequest);
            if (!result.Succeeded)
                return Result<bool>.Failure(result.Errors, result.StatusCode);

            await _auditService.LogActionAsync(
                "CreateTenantWithPlan",
                "Tenant",
                result.Data!.Id.ToString(),
                result.Data.Id,
                $"Created tenant '{result.Data.Name}' with plan '{plan.Name}'. Reason: {request.Reason}",
                JsonSerializer.Serialize(request));

            return Result<bool>.Success(true, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant with plan");
            return Result<bool>.Failure("An error occurred while creating tenant with plan", 500);
        }
    }

    public async Task<Result<bool>> MigrateTenantToNewPlanAsync(Guid tenantId, Guid newPlanId, string reason)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _context.Tenants
                .Include(t => t.Plan)
                .FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return Result<bool>.Failure($"Tenant with ID '{tenantId}' not found", 404);

            // Validate new plan exists
            var newPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == newPlanId);
            if (newPlan == null)
                return Result<bool>.Failure($"Plan with ID '{newPlanId}' not found", 404);

            var oldPlanName = tenant.Plan?.Name ?? "Unknown";
            var newPlanName = newPlan.Name;

            // Update tenant's plan
            tenant.PlanId = newPlanId;
            tenant.LastModified = DateTime.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "MigrateTenantToNewPlan",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Migrated tenant '{tenant.Name}' from plan '{oldPlanName}' to plan '{newPlanName}'. Reason: {reason}",
                JsonSerializer.Serialize(new { OldPlanId = tenant.PlanId, NewPlanId = newPlanId, Reason = reason }));

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating tenant to new plan");
            return Result<bool>.Failure("An error occurred while migrating tenant to new plan", 500);
        }
    }

    // Analytics and reporting
    public async Task<Result<SuperAdminAnalyticsDto>> GetSuperAdminAnalyticsAsync()
    {
        try
        {
            // Try to get from cache (15 minutes for analytics)
            var cacheKey = CacheKeys.SuperAdminAnalytics();
            var cached = await _cache.GetAsync<SuperAdminAnalyticsDto>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Returning cached super admin analytics");
                return Result<SuperAdminAnalyticsDto>.Success(cached, 200);
            }

            // Get tenant analytics directly from database
            var totalTenants = await _context.Tenants.CountAsync();
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive);
            var suspendedTenants = await _context.Tenants.CountAsync(t => !t.IsActive);

            var tenantAnalytics = new TenantAnalyticsDto
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                SuspendedTenants = suspendedTenants,
                TrialTenants = 0, // Would need license type filtering
                PremiumTenants = 0, // Would need license type filtering
                EnterpriseTenants = 0, // Would need license type filtering
                AverageTenantValue = 0m, // Would need billing data
                TotalRevenue = 0m, // Would need billing data
                ChurnRate = 0.05m, // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            var planUsage = await _planService.GetPlanUsageAsync();
            var moduleUsage = await _moduleService.GetModuleUsageAsync();
            var licenseAnalytics = await _licenseService.GetLicenseAnalyticsAsync();

            var analytics = new SuperAdminAnalyticsDto
            {
                TenantAnalytics = tenantAnalytics,
                PlanUsage = planUsage.Succeeded ? planUsage.Data ?? [] : [],
                ModuleUsage = moduleUsage.Succeeded ? moduleUsage.Data ?? [] : [],
                LicenseAnalytics = licenseAnalytics.Succeeded ? licenseAnalytics.Data! : new LicenseAnalyticsDto(),
                GeneratedAt = DateTime.UtcNow
            };

            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, analytics, TimeSpan.FromMinutes(15));

            return Result<SuperAdminAnalyticsDto>.Success(analytics, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting super admin analytics");
            return Result<SuperAdminAnalyticsDto>.Failure("An error occurred while getting analytics", 500);
        }
    }

    public async Task<Result<SystemHealthDto>> GetSystemHealthAsync()
    {
        try
        {
            // Try to get from cache (5 minutes for system health)
            var cacheKey = CacheKeys.SystemHealth();
            var cached = await _cache.GetAsync<SystemHealthDto>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Returning cached system health");
                return Result<SystemHealthDto>.Success(cached, 200);
            }

            var totalTenants = await _context.Tenants.CountAsync();
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive);
            var totalPlans = await _context.Plans.CountAsync();
            var activePlans = await _context.Plans.CountAsync(p => p.IsActive);
            var totalModules = await _context.Modules.CountAsync();
            var activeModules = await _context.Modules.CountAsync(m => m.IsActive);
            var totalLicenses = await _context.Licenses.CountAsync();
            var activeLicenses = await _context.Licenses.CountAsync(l => l.Status == LicenseStatusType.Active);

            var health = new SystemHealthDto
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                SuspendedTenants = totalTenants - activeTenants,
                TotalPlans = totalPlans,
                ActivePlans = activePlans,
                TotalLicenses = totalLicenses,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = await _context.Licenses.CountAsync(l => l.Status == LicenseStatusType.Expired),
                ExpiringLicenses = await _context.Licenses.CountAsync(l => l.ExpirationDate <= DateTime.UtcNow.AddDays(30)),
                LastUpdated = DateTime.UtcNow
            };

            // Cache for 5 minutes (health data should be relatively fresh)
            await _cache.SetAsync(cacheKey, health, TimeSpan.FromMinutes(5));

            return Result<SystemHealthDto>.Success(health, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health");
            return Result<SystemHealthDto>.Failure("An error occurred while getting system health", 500);
        }
    }

    public async Task<Result<UsageReportDto>> GetUsageReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            // Try to get from cache (30 minutes for usage reports)
            var cacheKey = CacheKeys.UsageReport(startDate, endDate);
            var cached = await _cache.GetAsync<UsageReportDto>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Returning cached usage report for {StartDate} to {EndDate}", startDate, endDate);
                return Result<UsageReportDto>.Success(cached, 200);
            }

            var tenantCount = await _context.Tenants
                .Where(t => t.Created >= startDate && t.Created <= endDate)
                .CountAsync();

            var planUsage = await _context.Tenants
                .Include(t => t.Plan)
                .Where(t => t.Created >= startDate && t.Created <= endDate)
                .GroupBy(t => t.Plan.Name)
                .Select(g => new PlanUsageDto
                {
                    PlanName = g.Key,
                    TenantCount = g.Count()
                })
                .ToListAsync();

            var report = new UsageReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = tenantCount, // Simplified - would need actual request data
                SuccessfulRequests = tenantCount, // Simplified
                FailedRequests = 0, // Simplified
                AverageResponseTime = 0m, // Would need actual metrics
                RequestsByEndpoint = new Dictionary<string, int>(),
                RequestsByTenant = new Dictionary<string, int>(),
                GeneratedAt = DateTime.UtcNow
            };

            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, report, TimeSpan.FromMinutes(30));

            return Result<UsageReportDto>.Success(report, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage report");
            return Result<UsageReportDto>.Failure("An error occurred while getting usage report", 500);
        }
    }

    // Bulk operations
    public async Task<Result<BulkOperationResponse>> BulkUpdateTenantsAsync(BulkTenantUpdateRequest[] updates)
    {
        try
        {
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();
            var failedItems = new Dictionary<Guid, string>();

            foreach (var update in updates)
            {
                try
                {
                    var result = await _tenantService.UpdateTenantAsync(update.Id, update.Data);
                    if (result.Succeeded)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.AddRange(result.Errors);
                        failedItems[update.Id] = string.Join(", ", result.Errors);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add(ex.Message);
                    failedItems[update.Id] = ex.Message;
                }
            }

            var response = new BulkOperationResponse
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors,
                FailedItems = failedItems
            };

            await _auditService.LogActionAsync(
                "BulkUpdateTenants",
                "Tenant",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updates.Length} tenants. Success: {successCount}, Failures: {failureCount}",
                JsonSerializer.Serialize(updates));

            return Result<BulkOperationResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk tenant update");
            return Result<BulkOperationResponse>.Failure("An error occurred during bulk tenant update", 500);
        }
    }

    public async Task<Result<BulkOperationResponse>> BulkUpdatePlansAsync(BulkPlanUpdateRequest[] updates)
    {
        try
        {
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();
            var failedItems = new Dictionary<Guid, string>();

            foreach (var update in updates)
            {
                try
                {
                    var result = await _planService.UpdatePlanAsync(update.Id, update.Data);
                    if (result.Succeeded)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.AddRange(result.Errors);
                        failedItems[update.Id] = string.Join(", ", result.Errors);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add(ex.Message);
                    failedItems[update.Id] = ex.Message;
                }
            }

            var response = new BulkOperationResponse
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors,
                FailedItems = failedItems
            };

            await _auditService.LogActionAsync(
                "BulkUpdatePlans",
                "Plan",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updates.Length} plans. Success: {successCount}, Failures: {failureCount}",
                JsonSerializer.Serialize(updates));

            return Result<BulkOperationResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk plan update");
            return Result<BulkOperationResponse>.Failure("An error occurred during bulk plan update", 500);
        }
    }

    public async Task<Result<BulkOperationResponse>> BulkUpdateModulesAsync(BulkModuleUpdateRequest[] updates)
    {
        try
        {
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();
            var failedItems = new Dictionary<Guid, string>();

            foreach (var update in updates)
            {
                try
                {
                    var result = await _moduleService.UpdateModuleAsync(update.Id, update.Data);
                    if (result.Succeeded)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.AddRange(result.Errors);
                        failedItems[update.Id] = string.Join(", ", result.Errors);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add(ex.Message);
                    failedItems[update.Id] = ex.Message;
                }
            }

            var response = new BulkOperationResponse
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors,
                FailedItems = failedItems
            };

            await _auditService.LogActionAsync(
                "BulkUpdateModules",
                "Module",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updates.Length} modules. Success: {successCount}, Failures: {failureCount}",
                JsonSerializer.Serialize(updates));

            return Result<BulkOperationResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk module update");
            return Result<BulkOperationResponse>.Failure("An error occurred during bulk module update", 500);
        }
    }

    public async Task<Result<BulkOperationResponse>> BulkUpdateLicensesAsync(BulkLicenseUpdateRequest[] updates)
    {
        try
        {
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();
            var failedItems = new Dictionary<Guid, string>();

            foreach (var update in updates)
            {
                try
                {
                    var result = await _licenseService.UpdateLicenseAsync(update.Id, update.Data);
                    if (result.Succeeded)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.AddRange(result.Errors);
                        failedItems[update.Id] = string.Join(", ", result.Errors);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add(ex.Message);
                    failedItems[update.Id] = ex.Message;
                }
            }

            var response = new BulkOperationResponse
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors,
                FailedItems = failedItems
            };

            await _auditService.LogActionAsync(
                "BulkUpdateLicenses",
                "License",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updates.Length} licenses. Success: {successCount}, Failures: {failureCount}",
                JsonSerializer.Serialize(updates));

            return Result<BulkOperationResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk license update");
            return Result<BulkOperationResponse>.Failure("An error occurred during bulk license update", 500);
        }
    }
}
