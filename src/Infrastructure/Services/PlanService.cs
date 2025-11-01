using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

public class PlanService(ApplicationDbContext context, ILogger<PlanService> logger, IMapper mapper, ISuperAdminAuditService auditService, IAuthService authService) : IPlanService
{

    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<PlanService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly IAuthService _authService = authService;

    public async Task<Result<PlanResponseDto>> CreatePlanAsync(CreatePlanRequest request)
    {
        try
        {
            var existingPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == request.Name);

            if (existingPlan != null)
                return Result<PlanResponseDto>.Failure($"Plan with name '{request.Name}' already exists", 400);

            var plan = _mapper.Map<Plan>(request);
            plan.Id = Guid.NewGuid();
            plan.CreatedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CreatedBy = "SuperAdmin";
            plan.UpdatedBy = "SuperAdmin";
            _context.Plans.Add(plan);

            // Create plan limits
            var limits = _mapper.Map<Limits>(request.Limits);
            limits.Id = Guid.NewGuid();
            limits.CreatedBy = "SuperAdmin";
            _context.Limits.Add(limits);

            // Link limits to plan
            plan.LimitsId = limits.Id;

            // Create PlanModule relationships
            foreach (var moduleId in request.ModuleIds)
            {
                // Validate that the module exists
                var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
                if (module == null)
                {
                    return Result<PlanResponseDto>.Failure($"Module with ID '{moduleId}' not found", 400);
                }

                var planModule = new Domain.Entities.PlanModule
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    ModuleId = moduleId,
                    IsRequired = false, // Default to not required
                    DisplayOrder = 0,
                    AddedAt = DateTime.UtcNow,
                    AddedBy = "SuperAdmin"
                };

                _context.PlanModules.Add(planModule);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "CreatePlan",
                "Plan",
                plan.Id.ToString(),
                plan.Id,
                $"Created plan '{request.Name}' with {request.ModuleIds.Length} modules",
                System.Text.Json.JsonSerializer.Serialize(request));

            // Reload the plan with all includes for proper mapping
            var createdPlan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == plan.Id);

            var planResponse = _mapper.Map<PlanResponseDto>(createdPlan);

            return Result<PlanResponseDto>.Success(planResponse, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plan: {PlanName}", request.Name);
            return Result<PlanResponseDto>.Failure("An error occurred while creating the plan", 500);
        }
    }

    public async Task<Result<PaginatedResult<PlanResponseDto>>> GetPlansAsync(PlanListRequest request)
    {
        try
        {
            var query = _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(p => p.Name.Contains(request.Query) ||
                        p.Description!.Contains(request.Query));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.BillingCycle))
            {
                if (Enum.TryParse<Domain.Enums.BillingCycle>(request.BillingCycle, true, out var billingCycle))
                {
                    query = query.Where(p => p.BillingCycle == billingCycle);
                }
            }

            // Apply sorting
            query = request.SortBy.ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                "updatedat" => request.SortDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
                "isactive" => request.SortDescending ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
                "ispopular" => request.SortDescending ? query.OrderByDescending(p => p.IsPopular) : query.OrderBy(p => p.IsPopular),
                _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var plans = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync();

            // Map to DTOs
            var planDtos = new List<PlanResponseDto>();
            foreach (var plan in plans)
            {
                var planDto = _mapper.Map<PlanResponseDto>(plan);
                planDtos.Add(planDto);
            }

            var paginatedResult = new PaginatedResult<PlanResponseDto>
            {
                Items = planDtos,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size
            };

            return Result<PaginatedResult<PlanResponseDto>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plans");
            return Result<PaginatedResult<PlanResponseDto>>.Failure("An error occurred while getting plans", 500);
        }
    }

    public async Task<Result<PlanResponseDto>> GetPlanByIdAsync(Guid planId)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<PlanResponseDto>.Failure("Plan not found", 404);

            var planDto = _mapper.Map<PlanResponseDto>(plan);

            return Result<PlanResponseDto>.Success(planDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan by ID: {PlanId}", planId);
            return Result<PlanResponseDto>.Failure("An error occurred while getting the plan", 500);
        }
    }

    public async Task<Result<PlanResponseDto>> UpdatePlanAsync(Guid planId, UpdatePlanRequest request)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<PlanResponseDto>.Failure("Plan not found", 404);

            // Check if name is being changed and if it conflicts with existing plan
            if (!string.IsNullOrEmpty(request.Name) && request.Name != plan.Name)
            {
                var existingPlan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.Name == request.Name && p.Id != planId);

                if (existingPlan != null)
                    return Result<PlanResponseDto>.Failure($"Plan with name '{request.Name}' already exists", 400);
            }

            // Update basic plan properties
            if (!string.IsNullOrEmpty(request.Name))
                plan.Name = request.Name;

            if (request.Description != null)
                plan.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Currency))
                plan.Currency = request.Currency;

            if (request.BillingCycle.HasValue)
                plan.BillingCycle = request.BillingCycle.Value;

            if (request.IsActive.HasValue)
                plan.IsActive = request.IsActive.Value;

            if (request.IsPopular.HasValue)
                plan.IsPopular = request.IsPopular.Value;

            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            // Update modules if provided
            if (request.Modules != null && request.Modules.Length > 0)
            {
                // Remove existing plan modules
                _context.PlanModules.RemoveRange(plan.PlanModules);

                // Add new plan modules
                foreach (var module in request.Modules!)
                {
                    // Validate that the module exists
                    var moduleEntity = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
                    if (moduleEntity == null)
                    {
                        return Result<PlanResponseDto>.Failure($"Module with ID '{module.Id}' not found", 400);
                    }

                    var planModule = new Domain.Entities.PlanModule
                    {
                        Id = Guid.NewGuid(),
                        PlanId = plan.Id,
                        ModuleId = moduleEntity.Id,
                        IsRequired = module.IsRequired,
                        DisplayOrder = module.DisplayOrder,
                        AddedAt = DateTime.UtcNow,
                        AddedBy = "SuperAdmin"
                    };

                    _context.PlanModules.Add(planModule);
                }
            }

            // Update limits if provided
            if (request.Limits != null)
            {
                if (plan.Limits != null)
                {
                    // Update existing limits
                    _mapper.Map(request.Limits, plan.Limits);
                }
                else
                {
                    // Create new limits
                    var limits = _mapper.Map<Limits>(request.Limits);
                    limits.Id = Guid.NewGuid();
                    limits.CreatedBy = "SuperAdmin";
                    _context.Limits.Add(limits);
                    plan.LimitsId = limits.Id;
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "UpdatePlan",
                "Plan",
                planId.ToString(),
                planId,
                $"Updated plan '{plan.Name}'",
                System.Text.Json.JsonSerializer.Serialize(request));

            // Return updated plan
            var updatedPlan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            var planDto = _mapper.Map<PlanResponseDto>(updatedPlan);

            return Result<PlanResponseDto>.Success(planDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plan: {PlanId}", planId);
            return Result<PlanResponseDto>.Failure("An error occurred while updating the plan", 500);
        }
    }

    public async Task<Result<bool>> DeletePlanAsync(Guid planId)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                .Include(p => p.Limits)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<bool>.Failure("Plan not found", 404);

            // Check if plan is being used by any tenants
            var tenantsUsingPlan = await _context.Tenants
                .AnyAsync(t => t.PlanId == planId);

            if (tenantsUsingPlan)
                return Result<bool>.Failure("Cannot delete plan that is currently being used by tenants", 400);

            // Remove related entities first (due to foreign key constraints)
            _context.PlanModules.RemoveRange(plan.PlanModules);

            // Remove the plan
            _context.Plans.Remove(plan);

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "DeletePlan",
                "Plan",
                planId.ToString(),
                planId,
                $"Deleted plan '{plan.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan: {PlanId}", planId);
            return Result<bool>.Failure("An error occurred while deleting the plan", 500);
        }
    }

    // IPlanService interface methods
    public async Task<Result<PlanResponseDto>> AssignLimitsAsync(Guid planId, Guid limitsId)
    {
        try
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
                return Result<PlanResponseDto>.Failure("Plan not found", 404);

            var limits = await _context.Limits.FirstOrDefaultAsync(l => l.Id == limitsId);
            if (limits == null)
                return Result<PlanResponseDto>.Failure("Limits not found", 404);

            plan.LimitsId = limitsId;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "AssignLimits",
                "Plan",
                planId.ToString(),
                planId,
                $"Assigned limits '{limits.Name}' to plan '{plan.Name}'");

            var updatedPlan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            var planDto = _mapper.Map<PlanResponseDto>(updatedPlan);
            return Result<PlanResponseDto>.Success(planDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning limits to plan: {PlanId}", planId);
            return Result<PlanResponseDto>.Failure("An error occurred while assigning limits", 500);
        }
    }

    public async Task<Result<PlanResponseDto>> UpdateLimitsAsync(Guid planId, UpdateLimitsRequest request)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.Limits)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<PlanResponseDto>.Failure("Plan not found", 404);

            if (plan.Limits == null)
                return Result<PlanResponseDto>.Failure("Plan has no limits assigned", 400);

            _mapper.Map(request, plan.Limits);
            plan.Limits.LastModifiedBy = "SuperAdmin";

            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "UpdateLimits",
                "Plan",
                planId.ToString(),
                planId,
                $"Updated limits for plan '{plan.Name}'");

            var updatedPlan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            var planDto = _mapper.Map<PlanResponseDto>(updatedPlan);
            return Result<PlanResponseDto>.Success(planDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating limits for plan: {PlanId}", planId);
            return Result<PlanResponseDto>.Failure("An error occurred while updating limits", 500);
        }
    }

    public async Task<Result<PlanUsageDto[]>> GetPlanUsageAsync()
    {
        try
        {
            var plans = await _context.Plans
                .Include(p => p.Tenants)
                .Include(pm => pm.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .AsNoTracking()
                .ToListAsync();

            var planUsage = plans.Select(plan =>
            {
                var tenantCount = plan.Tenants.Count;
                var totalModulePrice = plan.PlanModules.Sum(m => m.Module?.Pricing?.Price ?? 0);
                var totalRevenue = totalModulePrice * tenantCount;
                var averageTenantValue = tenantCount > 0 ? totalRevenue / tenantCount : 0;

                return new PlanUsageDto
                {
                    PlanId = plan.Id,
                    PlanName = plan.Name,
                    TenantCount = tenantCount,
                    TotalRevenue = totalRevenue,
                    AverageTenantValue = averageTenantValue,
                    ChurnRate = 0.05m,
                    LastUpdated = DateTime.UtcNow
                };
            }).ToArray();

            return Result<PlanUsageDto[]>.Success(planUsage, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan usage");
            return Result<PlanUsageDto[]>.Failure("An error occurred while getting plan usage", 500);
        }
    }

    public async Task<Result<bool>> BulkUpdatePlansAsync(BulkPlanUpdateRequest[] updates)
    {
        try
        {
            var planIds = updates.Select(u => u.Id).ToList();
            var plans = await _context.Plans
                .Include(p => p.PlanModules)
                .Where(p => planIds.Contains(p.Id))
                .ToListAsync();

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var update in updates)
            {
                var plan = plans.FirstOrDefault(p => p.Id == update.Id);
                if (plan == null)
                {
                    errors.Add($"Plan with ID {update.Id} not found");
                    continue;
                }

                try
                {
                    // Update plan properties
                    if (!string.IsNullOrEmpty(update.Data.Name))
                        plan.Name = update.Data.Name;

                    if (update.Data.Description != null)
                        plan.Description = update.Data.Description;

                    if (!string.IsNullOrEmpty(update.Data.Currency))
                        plan.Currency = update.Data.Currency;

                    if (update.Data.BillingCycle.HasValue)
                        plan.BillingCycle = update.Data.BillingCycle.Value;

                    if (update.Data.IsActive.HasValue)
                        plan.IsActive = update.Data.IsActive.Value;

                    if (update.Data.IsPopular.HasValue)
                        plan.IsPopular = update.Data.IsPopular.Value;

                    plan.UpdatedAt = DateTime.UtcNow;
                    plan.UpdatedBy = "SuperAdmin";

                    // Update modules if provided
                    if (update.Data.Modules != null && update.Data.Modules.Length > 0)
                    {
                        // Remove existing plan modules
                        _context.PlanModules.RemoveRange(plan.PlanModules);

                        // Add new plan modules
                        foreach (var module in update.Data.Modules)
                        {
                            // Validate that the module exists
                            var moduleEntity = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
                            if (moduleEntity == null)
                            {
                                errors.Add($"Module with ID '{module.Id}' not found for plan '{plan.Name}'");
                                continue;
                            }

                            var planModule = new Domain.Entities.PlanModule
                            {
                                Id = Guid.NewGuid(),
                                PlanId = plan.Id,
                                ModuleId = module.Id,
                                IsRequired = module.IsRequired,
                                DisplayOrder = module.DisplayOrder,
                                AddedAt = DateTime.UtcNow,
                                AddedBy = "SuperAdmin"
                            };

                            _context.PlanModules.Add(planModule);
                        }
                    }

                    // Update limits if provided
                    if (update.Data.Limits != null)
                    {
                        if (plan.Limits != null)
                        {
                            // Update existing limits
                            _mapper.Map(update.Data.Limits, plan.Limits);
                            plan.Limits.LastModifiedBy = "SuperAdmin";
                        }
                        else
                        {
                            // Create new limits
                            var limits = _mapper.Map<Limits>(update.Data.Limits);
                            limits.Id = Guid.NewGuid();
                            limits.CreatedBy = "SuperAdmin";
                            _context.Limits.Add(limits);
                            plan.LimitsId = limits.Id;
                        }
                    }

                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating plan {PlanId} in bulk operation", update.Id);
                    errors.Add($"Failed to update plan '{plan.Name}': {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "BulkUpdatePlans",
                "Plan",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updatedCount} plans. Errors: {errors.Count}",
                System.Text.Json.JsonSerializer.Serialize(updates));

            if (errors.Any())
            {
                return Result<bool>.Failure($"Bulk update completed with {errors.Count} errors: {string.Join("; ", errors)}", 207);
            }

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update plans");
            return Result<bool>.Failure("An error occurred during bulk update of plans", 500);
        }
    }

    public async Task<Result<bool>> AssignModuleToPlanAsync(Guid planId, Guid moduleId)
    {
        try
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan == null)
                return Result<bool>.Failure("Plan not found", 404);

            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null)
                return Result<bool>.Failure("Module not found", 404);

            // Check if module is already assigned to this plan
            var existingPlanModule = await _context.PlanModules
                .FirstOrDefaultAsync(pm => pm.PlanId == planId && pm.ModuleId == moduleId);

            if (existingPlanModule != null)
                return Result<bool>.Failure("Module is already assigned to this plan", 400);

            // Create new PlanModule relationship
            var planModule = new Domain.Entities.PlanModule
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                ModuleId = moduleId,
                IsRequired = false, // Default to not required
                DisplayOrder = 0,
                AddedAt = DateTime.UtcNow,
                AddedBy = "SuperAdmin"
            };

            _context.PlanModules.Add(planModule);

            // Update plan metadata
            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "AssignModuleToPlan",
                "Plan",
                planId.ToString(),
                planId,
                $"Assigned module '{module.Name}' to plan '{plan.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning module {ModuleId} to plan {PlanId}", moduleId, planId);
            return Result<bool>.Failure("An error occurred while assigning module to plan", 500);
        }
    }

    public async Task<Result<bool>> RemoveModuleFromPlanAsync(Guid planId, Guid moduleId)
    {
        try
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan == null)
                return Result<bool>.Failure("Plan not found", 404);

            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null)
                return Result<bool>.Failure("Module not found", 404);

            // Find the PlanModule relationship
            var planModule = await _context.PlanModules
                .FirstOrDefaultAsync(pm => pm.PlanId == planId && pm.ModuleId == moduleId);

            if (planModule == null)
                return Result<bool>.Failure("Module is not assigned to this plan", 400);

            // Remove the PlanModule relationship
            _context.PlanModules.Remove(planModule);

            // Update plan metadata
            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "RemoveModuleFromPlan",
                "Plan",
                planId.ToString(),
                planId,
                $"Removed module '{module.Name}' from plan '{plan.Name}'",
                null);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing module {ModuleId} from plan {PlanId}", moduleId, planId);
            return Result<bool>.Failure("An error occurred while removing module from plan", 500);
        }
    }
}
