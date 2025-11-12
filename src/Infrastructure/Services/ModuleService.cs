using System.Text.Json;
using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

public class ModuleService(ApplicationDbContext context, ILogger<ModuleService> logger, IMapper mapper, ISuperAdminAuditService auditService, ICacheService cache) : IModuleService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<ModuleService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly ICacheService _cache = cache;

    public async Task<Result<ModuleResponseDto>> CreateModuleAsync(CreateModuleRequest request)
    {
        try
        {
            var existingModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.Name == request.Name);

            if (existingModule != null)
                return Result<ModuleResponseDto>.Failure($"Module with name '{request.Name}' already exists", 400);

            var module = _mapper.Map<Module>(request);
            module.Id = Guid.NewGuid();
            module.CreatedAt = DateTime.UtcNow;
            module.UpdatedAt = DateTime.UtcNow;
            module.CreatedBy = "SuperAdmin";
            module.UpdatedBy = "SuperAdmin";

            // Store dependencies as JSON
            if (request.Dependencies.Length > 0)
            {
                module.Dependencies = JsonSerializer.Serialize(request.Dependencies);
            }

            _context.Modules.Add(module);

            // Create module features
            foreach (var featureRequest in request.Features)
            {
                var feature = _mapper.Map<ModuleFeature>(featureRequest);
                feature.Id = Guid.NewGuid();
                feature.ModuleId = module.Id;
                feature.Module = module;

                // Store configuration as JSON
                if (featureRequest.Configuration != null)
                {
                    feature.Configuration = JsonSerializer.Serialize(featureRequest.Configuration);
                }

                _context.ModuleFeatures.Add(feature);
            }

            // Create module pricing
            var pricing = _mapper.Map<ModulePricing>(request.Pricing);
            pricing.Id = Guid.NewGuid();
            pricing.ModuleId = module.Id;
            pricing.Module = module;
            _context.ModulePricing.Add(pricing);

            await _context.SaveChangesAsync();

            // Invalidate all modules list caches
            await InvalidateAllModulesListCacheAsync();

            await _auditService.LogActionAsync(
                "CreateModule",
                "Module",
                module.Id.ToString(),
                module.Id,
                $"Created module '{request.Name}' with {request.Features.Length} features",
                JsonSerializer.Serialize(request));

            var moduleResponse = _mapper.Map<ModuleResponseDto>(module);

            // Handle dependencies deserialization manually
            if (!string.IsNullOrEmpty(module.Dependencies))
            {
                moduleResponse = moduleResponse with
                {
                    Dependencies = JsonSerializer.Deserialize<string[]>(module.Dependencies ?? string.Empty) ?? Array.Empty<string>()
                };
            }

            // Handle features configuration deserialization
            var featuresWithConfig = new List<ModuleFeatureResponseDto>();
            foreach (var feature in module.Features)
            {
                var featureDto = _mapper.Map<ModuleFeatureResponseDto>(feature);
                if (!string.IsNullOrEmpty(feature.Configuration))
                {
                    featureDto = featureDto with
                    {
                        Configuration = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(feature.Configuration)
                    };
                }
                featuresWithConfig.Add(featureDto);
            }
            moduleResponse = moduleResponse with { Features = featuresWithConfig.ToArray() };

            return Result<ModuleResponseDto>.Success(moduleResponse, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating module: {ModuleName}", request.Name);
            return Result<ModuleResponseDto>.Failure("An error occurred while creating the module", 500);
        }
    }

    /// <summary>
    /// Invalidates all module-related caches
    /// </summary>
    private async Task InvalidateAllModulesListCacheAsync()
    {
        // Invalidate modules list caches
        await _cache.RemoveByPatternAsync("modules:list:*");
        await _cache.RemoveAsync(CacheKeys.AllModules());
        await _cache.RemoveAsync(CacheKeys.ModuleUsage());
        _logger.LogDebug("Invalidated all modules list caches");
    }

    /// <summary>
    /// Invalidates cache for a specific module
    /// </summary>
    private async Task InvalidateModuleCacheAsync(Guid moduleId)
    {
        await _cache.RemoveAsync(CacheKeys.Module(moduleId));
        await InvalidateAllModulesListCacheAsync();
        // Also invalidate plan caches since modules are part of plans
        await _cache.RemoveByPatternAsync("plans:*");
        _logger.LogDebug("Invalidated cache for module: {ModuleId}", moduleId);
    }

    public async Task<Result<PaginatedResult<ModuleResponseDto>>> GetModulesAsync(ModuleListRequest request)
    {
        try
        {
            // Try to get from cache
            var cacheKey = CacheKeys.ModulesList(
                request.Page,
                request.Size,
                request.Category,
                request.IsActive,
                request.IsCore,
                request.Query,
                request.SortBy,
                request.SortDescending);

            var cached = await _cache.GetAsync<PaginatedResult<ModuleResponseDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Returning cached modules list for key: {CacheKey}", cacheKey);
                return Result<PaginatedResult<ModuleResponseDto>>.Success(cached, 200);
            }

            var query = _context.Modules
                .Include(m => m.Features)
                .Include(m => m.Pricing)
                .AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(m => m.Name.Contains(request.Query) ||
                        m.Description!.Contains(request.Query) ||
                        m.Category.Contains(request.Query));
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(m => m.Category == request.Category);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(m => m.IsActive == request.IsActive.Value);
            }

            if (request.IsCore.HasValue)
            {
                query = query.Where(m => m.IsCore == request.IsCore.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
                "category" => request.SortDescending ? query.OrderByDescending(m => m.Category) : query.OrderBy(m => m.Category),
                "createdat" => request.SortDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
                "updatedat" => request.SortDescending ? query.OrderByDescending(m => m.UpdatedAt) : query.OrderBy(m => m.UpdatedAt),
                "isactive" => request.SortDescending ? query.OrderByDescending(m => m.IsActive) : query.OrderBy(m => m.IsActive),
                "iscore" => request.SortDescending ? query.OrderByDescending(m => m.IsCore) : query.OrderBy(m => m.IsCore),
                _ => request.SortDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var modules = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync();

            // Map to DTOs
            var moduleDtos = new List<ModuleResponseDto>();
            foreach (var module in modules)
            {
                var moduleDto = _mapper.Map<ModuleResponseDto>(module);

                // Handle dependencies deserialization manually
                if (!string.IsNullOrEmpty(module.Dependencies))
                {
                    moduleDto = moduleDto with
                    {
                        Dependencies = System.Text.Json.JsonSerializer.Deserialize<string[]>(module.Dependencies) ?? Array.Empty<string>()
                    };
                }

                // Handle features configuration deserialization
                var featuresWithConfig = new List<ModuleFeatureResponseDto>();
                foreach (var feature in module.Features)
                {
                    var featureDto = _mapper.Map<ModuleFeatureResponseDto>(feature);
                    if (!string.IsNullOrEmpty(feature.Configuration))
                    {
                        featureDto = featureDto with
                        {
                            Configuration = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(feature.Configuration)
                        };
                    }
                    featuresWithConfig.Add(featureDto);
                }
                moduleDto = moduleDto with { Features = featuresWithConfig.ToArray() };

                moduleDtos.Add(moduleDto);
            }

            var paginatedResult = new PaginatedResult<ModuleResponseDto>
            {
                Items = moduleDtos,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size
            };

            // Cache for 24 hours (static data)
            await _cache.SetAsync(cacheKey, paginatedResult, TimeSpan.FromHours(24));

            return Result<PaginatedResult<ModuleResponseDto>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules");
            return Result<PaginatedResult<ModuleResponseDto>>.Failure("An error occurred while getting modules", 500);
        }
    }

    public async Task<Result<ModuleResponseDto>> GetModuleByIdAsync(Guid moduleId)
    {
        try
        {
            // Try to get from cache
            var cacheKey = CacheKeys.Module(moduleId);
            var cached = await _cache.GetAsync<ModuleResponseDto>(cacheKey);

            if (cached != null)
            {
                _logger.LogDebug("Returning cached module for ID: {ModuleId}", moduleId);
                return Result<ModuleResponseDto>.Success(cached, 200);
            }

            var module = await _context.Modules
                .Include(m => m.Features)
                .Include(m => m.Pricing)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null)
                return Result<ModuleResponseDto>.Failure("Module not found", 404);

            var moduleDto = _mapper.Map<ModuleResponseDto>(module);

            // Handle dependencies deserialization manually
            if (!string.IsNullOrEmpty(module.Dependencies))
            {
                moduleDto = moduleDto with
                {
                    Dependencies = JsonSerializer.Deserialize<string[]>(module.Dependencies ?? string.Empty) ?? Array.Empty<string>()
                };
            }

            // Handle features configuration deserialization
            var featuresWithConfig = new List<ModuleFeatureResponseDto>();
            foreach (var feature in module.Features)
            {
                var featureDto = _mapper.Map<ModuleFeatureResponseDto>(feature);
                if (!string.IsNullOrEmpty(feature.Configuration))
                {
                    featureDto = featureDto with
                    {
                        Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(feature.Configuration)
                    };
                }
                featuresWithConfig.Add(featureDto);
            }
            moduleDto = moduleDto with { Features = featuresWithConfig.ToArray() };

            // Cache for 24 hours (static data)
            await _cache.SetAsync(cacheKey, moduleDto, TimeSpan.FromHours(24));

            return Result<ModuleResponseDto>.Success(moduleDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module by ID: {ModuleId}", moduleId);
            return Result<ModuleResponseDto>.Failure("An error occurred while getting the module", 500);
        }
    }

    public async Task<Result<ModuleResponseDto>> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request)
    {
        try
        {
            var module = await _context.Modules
                .Include(m => m.Features)
                .Include(m => m.Pricing)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null)
                return Result<ModuleResponseDto>.Failure("Module not found", 404);

            // Check if name is being changed and if it conflicts with existing module
            if (!string.IsNullOrEmpty(request.Name) && request.Name != module.Name)
            {
                var existingModule = await _context.Modules
                    .FirstOrDefaultAsync(m => m.Name == request.Name && m.Id != moduleId);

                if (existingModule != null)
                    return Result<ModuleResponseDto>.Failure($"Module with name '{request.Name}' already exists", 400);
            }

            // Update basic module properties
            if (!string.IsNullOrEmpty(request.Name))
                module.Name = request.Name;

            if (request.Description != null)
                module.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Category))
                module.Category = request.Category;

            if (request.IsActive.HasValue)
                module.IsActive = request.IsActive.Value;

            if (request.IsCore.HasValue)
                module.IsCore = request.IsCore.Value;

            if (request.Dependencies != null)
            {
                module.Dependencies = request.Dependencies.Length > 0
                    ? System.Text.Json.JsonSerializer.Serialize(request.Dependencies)
                    : null;
            }

            module.UpdatedAt = DateTime.UtcNow;
            module.UpdatedBy = "SuperAdmin";

            // Update features if provided
            if (request.Features != null && request.Features.Length > 0)
            {
                // Remove existing features
                _context.ModuleFeatures.RemoveRange(module.Features);

                // Add new features
                foreach (var featureRequest in request.Features)
                {
                    var feature = _mapper.Map<ModuleFeature>(featureRequest);
                    feature.Id = Guid.NewGuid();
                    feature.ModuleId = module.Id;
                    feature.Module = module;

                    // Store configuration as JSON
                    if (featureRequest.Configuration != null)
                    {
                        feature.Configuration = System.Text.Json.JsonSerializer.Serialize(featureRequest.Configuration);
                    }

                    _context.ModuleFeatures.Add(feature);
                }
            }

            // Update pricing if provided
            if (request.Pricing != null)
            {
                if (module.Pricing != null)
                {
                    // Update existing pricing
                    _mapper.Map(request.Pricing, module.Pricing);
                }
                else
                {
                    // Create new pricing
                    var pricing = _mapper.Map<ModulePricing>(request.Pricing);
                    pricing.Id = Guid.NewGuid();
                    pricing.ModuleId = module.Id;
                    pricing.Module = module;
                    _context.ModulePricing.Add(pricing);
                }
            }

            await _context.SaveChangesAsync();

            // Invalidate caches
            await InvalidateModuleCacheAsync(moduleId);

            await _auditService.LogActionAsync(
                "UpdateModule",
                "Module",
                moduleId.ToString(),
                moduleId,
                $"Updated module '{module.Name}'",
                System.Text.Json.JsonSerializer.Serialize(request));

            // Return updated module
            var updatedModule = await _context.Modules
                .Include(m => m.Features)
                .Include(m => m.Pricing)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            var moduleDto = _mapper.Map<ModuleResponseDto>(updatedModule);

            // Handle dependencies deserialization manually
            if (!string.IsNullOrEmpty(updatedModule!.Dependencies))
            {
                moduleDto = moduleDto with
                {
                    Dependencies = System.Text.Json.JsonSerializer.Deserialize<string[]>(updatedModule.Dependencies) ?? Array.Empty<string>()
                };
            }

            // Handle features configuration deserialization
            var featuresWithConfig = new List<ModuleFeatureResponseDto>();
            foreach (var feature in updatedModule.Features)
            {
                var featureDto = _mapper.Map<ModuleFeatureResponseDto>(feature);
                if (!string.IsNullOrEmpty(feature.Configuration))
                {
                    featureDto = featureDto with
                    {
                        Configuration = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(feature.Configuration)
                    };
                }
                featuresWithConfig.Add(featureDto);
            }
            moduleDto = moduleDto with { Features = featuresWithConfig.ToArray() };

            return Result<ModuleResponseDto>.Success(moduleDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module: {ModuleId}", moduleId);
            return Result<ModuleResponseDto>.Failure("An error occurred while updating the module", 500);
        }
    }

    public async Task<Result<bool>> DeleteModuleAsync(Guid moduleId)
    {
        try
        {
            var module = await _context.Modules
                .Include(m => m.Features)
                .Include(m => m.Pricing)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null)
                return Result<bool>.Failure("Module not found", 404);

            // Check if module is core (cannot delete core modules)
            if (module.IsCore)
                return Result<bool>.Failure("Cannot delete core modules", 400);

            // Check if module is being used by any plans
            var plansUsingModule = await _context.Plans
                .AnyAsync(p => p.PlanModules!.Any(pm => pm.ModuleId == module.Id));

            if (plansUsingModule)
                return Result<bool>.Failure("Cannot delete module that is currently being used by plans", 400);

            // Remove related entities first (due to foreign key constraints)
            _context.ModuleFeatures.RemoveRange(module.Features);

            if (module.Pricing != null)
                _context.ModulePricing.Remove(module.Pricing);

            // Remove the module
            _context.Modules.Remove(module);

            await _context.SaveChangesAsync();

            // Invalidate caches
            await InvalidateModuleCacheAsync(moduleId);

            await _auditService.LogActionAsync(
                "DeleteModule",
                "Module",
                moduleId.ToString(),
                moduleId,
                $"Deleted module '{module.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module: {ModuleId}", moduleId);
            return Result<bool>.Failure("An error occurred while deleting the module", 500);
        }
    }

    // public async Task<Result<bool>> BulkUpdateModulesAsync(HotelManagement.Application.Core.ModuleManagement.Commands.BulkModuleUpdateRequest[] updates)
    // {
    //     try
    //     {
    //         var moduleIds = updates.Select(u => u.Id).ToList();
    //         var modules = await _context.Modules
    //             .Where(m => moduleIds.Contains(m.Id))
    //             .ToListAsync();

    //         var updatedCount = 0;
    //         var errors = new List<string>();

    //         foreach (var update in updates)
    //         {
    //             var module = modules.FirstOrDefault(m => m.Id == update.Id);
    //             if (module == null)
    //             {
    //                 errors.Add($"Module with ID {update.Id} not found");
    //                 continue;
    //             }

    //             try
    //             {
    //                 // Update module properties
    //                 if (!string.IsNullOrEmpty(update.Data.Name))
    //                     module.Name = update.Data.Name;

    //                 if (update.Data.Description != null)
    //                     module.Description = update.Data.Description;

    //                 if (!string.IsNullOrEmpty(update.Data.Category))
    //                     module.Category = update.Data.Category;

    //                 if (update.Data.IsActive.HasValue)
    //                     module.IsActive = update.Data.IsActive.Value;

    //                 if (update.Data.IsCore.HasValue)
    //                     module.IsCore = update.Data.IsCore.Value;

    //                 if (update.Data.Dependencies != null)
    //                 {
    //                     module.Dependencies = update.Data.Dependencies.Length > 0
    //                         ? System.Text.Json.JsonSerializer.Serialize(update.Data.Dependencies)
    //                         : null;
    //                 }

    //                 module.UpdatedAt = DateTime.UtcNow;
    //                 module.UpdatedBy = "SuperAdmin";
    //                 updatedCount++;
    //             }
    //             catch (Exception ex)
    //             {
    //                 _logger.LogError(ex, "Error updating module {ModuleId}", update.Id);
    //                 errors.Add($"Failed to update module {update.Id}: {ex.Message}");
    //             }
    //         }

    //         await _context.SaveChangesAsync();

    //         await _auditService.LogActionAsync(
    //             "BulkUpdateModules",
    //             "Module",
    //             "Bulk",
    //             Guid.Empty,
    //             $"Bulk updated {updatedCount} modules",
    //             System.Text.Json.JsonSerializer.Serialize(updates));

    //         if (errors.Any())
    //         {
    //             return Result<bool>.Failure($"Bulk update completed with {errors.Count} errors: {string.Join(", ", errors)}", 207);
    //         }

    //         return Result<bool>.Success(true, 200);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error in bulk update modules");
    //         return Result<bool>.Failure("An error occurred during bulk update of modules", 500);
    //     }
    // }

    public async Task<Result<ModuleUsageDto[]>> GetModuleUsageAsync()
    {
        try
        {
            // Try to get from cache (30 minutes for analytics data)
            var cacheKey = CacheKeys.ModuleUsage();
            var cached = await _cache.GetAsync<ModuleUsageDto[]>(cacheKey);

            if (cached != null)
            {
                _logger.LogDebug("Returning cached module usage analytics");
                return Result<ModuleUsageDto[]>.Success(cached, 200);
            }

            var modules = await _context.Modules
                .AsNoTracking()
                .ToListAsync();

            var moduleUsageList = new List<ModuleUsageDto>();

            foreach (var module in modules)
            {
                // Count tenants using this module
                var tenantCount = await _context.Tenants
                    .Include(t => t.Plan)
                    .CountAsync(t => t.Plan!.PlanModules!.Any(pm => pm.ModuleId == module.Id));

                // Calculate adoption rate (percentage of tenants using this module)
                var totalTenants = await _context.Tenants.CountAsync();
                var adoptionRate = totalTenants > 0 ? (decimal)tenantCount / totalTenants * 100 : 0;

                // Calculate revenue (simplified - would need actual billing data)
                var revenue = tenantCount * (module.Pricing?.Price ?? 0);

                moduleUsageList.Add(new ModuleUsageDto
                {
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    TenantCount = tenantCount,
                    AdoptionRate = adoptionRate,
                    Revenue = revenue,
                    LastUpdated = DateTime.UtcNow
                });
            }

            // Cache for 30 minutes (analytics data)
            await _cache.SetAsync(cacheKey, moduleUsageList.ToArray(), TimeSpan.FromMinutes(30));

            return Result<ModuleUsageDto[]>.Success(moduleUsageList.ToArray(), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module usage analytics");
            return Result<ModuleUsageDto[]>.Failure("An error occurred while getting module usage analytics", 500);
        }
    }

    public async Task<Result<bool>> AssignModuleToPlanAsync(Guid moduleId, Guid planId)
    {
        try
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId);

            if (module == null)
                return Result<bool>.Failure("Module not found", 404);

            if (plan == null)
                return Result<bool>.Failure("Plan not found", 404);

            var planModule = new PlanModule
            {
                Id = Guid.NewGuid(),
                ModuleId = module.Id,
                PlanId = plan.Id
            };

            _context.PlanModules.Add(planModule);

            await _context.SaveChangesAsync();

            // Invalidate caches for both module and plan
            await InvalidateModuleCacheAsync(moduleId);
            await _cache.RemoveAsync(CacheKeys.Plan(planId));
            await _cache.RemoveByPatternAsync("plans:list:*");

            await _auditService.LogActionAsync(
                "AssignModuleToPlan",
                "Module",
                moduleId.ToString(),
                moduleId,
                $"Assigned module '{module.Name}' to plan '{plan.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning module to plan: {ModuleId}", moduleId);
            return Result<bool>.Failure("An error occurred while assigning the module to the plan", 500);
        }
    }

    public async Task<Result<bool>> RemoveModuleFromPlanAsync(Guid moduleId, Guid planId)
    {
        try
        {
            var planModule = await _context.PlanModules
                .Include(pm => pm.Module)
                .Include(pm => pm.Plan)
                .FirstOrDefaultAsync(pm => pm.ModuleId == moduleId && pm.PlanId == planId);
            if (planModule == null)
                return Result<bool>.Failure("Module not found in plan", 404);

            _context.PlanModules.Remove(planModule);

            await _context.SaveChangesAsync();

            // Invalidate caches for both module and plan
            await InvalidateModuleCacheAsync(moduleId);
            await _cache.RemoveAsync(CacheKeys.Plan(planId));
            await _cache.RemoveByPatternAsync("plans:list:*");

            await _auditService.LogActionAsync(
                "RemoveModuleFromPlan",
                "Module",
                moduleId.ToString(),
                moduleId,
                $"Removed module '{planModule.Module.Name}' from plan '{planModule.Plan.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing module from plan: {ModuleId}", moduleId);
            return Result<bool>.Failure("An error occurred while removing the module from the plan", 500);
        }
    }
}
