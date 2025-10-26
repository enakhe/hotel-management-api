using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using HotelManagement.Domain.Entities.Data;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Domain.Constants;
using HotelManagement.Application.Common.Interfaces.Administrator;

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
    IMapper mapper) : ISuperAdminService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<SuperAdminService> _logger = logger;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;


    public async Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantRequest request)
    {
        try
        {
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == request.Identifier);

            if (existingTenant != null)
                return Result<TenantSummary>.Failure($"Tenant with identifier '{request.Identifier}' already exists", 400);

            var tenant = _mapper.Map<Tenant>(request);

            _context.Tenants.Add(tenant);

            foreach (var module in request.Modules!)
            {
                var feature = new TenantFeature
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    FeatureName = module,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    Tenant = tenant
                };
                _context.TenantFeatures.Add(feature);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "CreateTenant",
                "Tenant",
                tenant.Id.ToString(),
                tenant.Id,
                $"Created tenant '{request.Name}' with identifier '{request.Identifier}'",
                JsonSerializer.Serialize(request));

            var registerUserDto = new RegisterUserDto()
            {
                Email = tenant.Email!,
                Tenant = tenant.Identifier,
                Roles = [Roles.Administrator.ToString()]
            };

            var response = await _authService.RegisterAsync(registerUserDto);

            if (response.Succeeded)
                await _auditService.LogActionAsync(
                    "CreateTenantAdminUser",
                    "User",
                    tenant.Name,
                    tenant.Id,
                    $"Created initial admin user '{tenant.Email}' for tenant '{tenant.Name}'");

            var tenantSummary = new TenantSummary
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                IsActive = tenant.IsActive,
                LicenseStatus = tenant.LicenseStatus.ToString(),
                SubscriptionPlan = tenant.SubscriptionPlan ?? "Basic",
                Country = tenant.Country,
                Region = tenant.Region,


                CreatedAt = tenant.Created.DateTime,
                LastActivity = tenant.LastModified.DateTime,
                UserCount = 0,
                BranchCount = 0
            };

            return Result<TenantSummary>.Success(tenantSummary, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {Identifier}", request.Identifier);
            return Result<TenantSummary>.Failure("An error occurred while creating the tenant", 500);
        }
    }

    public async Task<Result<PaginatedResult<TenantSummary>>> GetTenantsAsync(TenantListRequest request)
    {
        try
        {
            var query = _context.Tenants.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(t => t.Name.Contains(request.Query) ||
                        t.Identifier.Contains(request.Query) ||
                        t.Email!.Contains(request.Query));
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<LicenseStatus>(request.Status, true, out var status))
                {
                    query = query.Where(t => t.LicenseStatus == status);
                }
            }

            if (!string.IsNullOrEmpty(request.Plan))
            {
                query = query.Where(t => t.SubscriptionPlan == request.Plan.ToString());
            }

            if (!string.IsNullOrEmpty(request.Region))
            {
                query = query.Where(t => t.Region == request.Region);
            }

            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(t => t.Created >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                query = query.Where(t => t.Created <= request.CreatedTo.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "identifier" => request.SortDescending ? query.OrderByDescending(t => t.Identifier) : query.OrderBy(t => t.Identifier),
                "createdat" => request.SortDescending ? query.OrderByDescending(t => t.Created) : query.OrderBy(t => t.Created),
                "status" => request.SortDescending ? query.OrderByDescending(t => t.LicenseStatus) : query.OrderBy(t => t.LicenseStatus),
                _ => query.OrderByDescending(t => t.Created)
            };

            var totalCount = await query.CountAsync();

            var pagedTenantIds = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(t => t.Id)
                .ToListAsync();

            var userCountsDict = await _context.Users
                .Where(u => u.TenantId.HasValue && pagedTenantIds.Contains(u.TenantId.Value))
                .GroupBy(u => u.TenantId!.Value)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.TenantId, g => g.Count);

            var branchCountsDict = await _context.Branches
                .Where(b => pagedTenantIds.Contains(b.TenantId))
                .GroupBy(b => b.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.TenantId, g => g.Count);

            var items = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync();

            var itemsWithCounts = items.Select(t => new TenantSummary
            {
                Id = t.Id,
                Name = t.Name,
                Identifier = t.Identifier,
                IsActive = t.IsActive,
                LicenseStatus = t.LicenseStatus.ToString(),
                SubscriptionPlan = t.SubscriptionPlan ?? "Basic",
                Country = t.Country,
                Region = t.Region,

                CreatedAt = t.Created.DateTime,
                LastActivity = t.LastModified.DateTime,
                UserCount = userCountsDict.GetValueOrDefault(t.Id, 0),
                BranchCount = branchCountsDict.GetValueOrDefault(t.Id, 0)
            }).ToList();

            var paginatedResult = new PaginatedResult<TenantSummary>
            {
                Items = itemsWithCounts,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size
            };

            return Result<PaginatedResult<TenantSummary>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenants list");
            return Result<PaginatedResult<TenantSummary>>.Failure("An error occurred while getting the tenants list", 500);
        }
    }

    public async Task<Result<TenantDetail>> GetTenantDetailAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Features)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return Result<TenantDetail>.Failure("Tenant not found", 404);

            var enabledModules = tenant.Features
                .Where(f => f.IsEnabled)
                .Select(f => f.FeatureName)
                .ToArray();

            var tenantDetails = new TenantDetail
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                Description = tenant.Description,
                Address = tenant.Address,
                ContactNumber = tenant.ContactNumber,
                Email = tenant.Email,
                TimeZone = tenant.TimeZone ?? "WAT",
                CurrencyCode = tenant.CurrencyCode ?? "NGN",
                LanguageCode = tenant.LanguageCode ?? "en",
                Country = tenant.Country,
                Region = tenant.Region,
                Industry = tenant.Industry,
                IsActive = tenant.IsActive,
                LicenseStatus = tenant.LicenseStatus.ToString(),
                LicenseExpiryDate = tenant.LicenseExpiryDate,
                SubscriptionPlan = tenant.SubscriptionPlan ?? SubscriptionPlan.Basic.ToString(),
                SubscriptionStartDate = tenant.SubscriptionStartDate,
                SubscriptionEndDate = tenant.SubscriptionEndDate,
                MaxUsers = tenant.MaxUsers,
                MaxBranches = tenant.MaxBranches,
                MaxRooms = tenant.MaxRooms,
                MaxReservations = tenant.MaxReservations,
                EnabledModules = enabledModules,
                CreatedAt = tenant.Created.DateTime,
                LastActivity = tenant.LastModified.DateTime
            };

            return Result<TenantDetail>.Success(tenantDetails, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant detail for {TenantId}", tenantId);
            return Result<TenantDetail>.Failure("An error occurred while getting the tenant detail", 500);
        }
    }

    public async Task<Result<bool>> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            var changes = new Dictionary<string, object>();

            // Update basic information
            if (request.Name != null && tenant.Name != request.Name)
            {
                changes["Name"] = new { From = tenant.Name, To = request.Name };
                tenant.Name = request.Name;
            }

            if (request.Description != null && tenant.Description != request.Description)
            {
                changes["Description"] = new { From = tenant.Description, To = request.Description };
                tenant.Description = request.Description;
            }

            if (request.Address != null && tenant.Address != request.Address)
            {
                changes["Address"] = new { From = tenant.Address, To = request.Address };
                tenant.Address = request.Address;
            }

            if (request.ContactNumber != null && tenant.ContactNumber != request.ContactNumber)
            {
                changes["ContactNumber"] = new { From = tenant.ContactNumber, To = request.ContactNumber };
                tenant.ContactNumber = request.ContactNumber;
            }

            if (request.Email != null && tenant.Email != request.Email)
            {
                changes["Email"] = new { From = tenant.Email, To = request.Email };
                tenant.Email = request.Email;
            }

            // Update configuration
            if (request.TimeZone != null && tenant.TimeZone != request.TimeZone)
            {
                changes["TimeZone"] = new { From = tenant.TimeZone, To = request.TimeZone };
                tenant.TimeZone = request.TimeZone;
            }

            if (request.CurrencyCode != null && tenant.CurrencyCode != request.CurrencyCode)
            {
                changes["CurrencyCode"] = new { From = tenant.CurrencyCode, To = request.CurrencyCode };
                tenant.CurrencyCode = request.CurrencyCode;
            }

            if (request.LanguageCode != null && tenant.LanguageCode != request.LanguageCode)
            {
                changes["LanguageCode"] = new { From = tenant.LanguageCode, To = request.LanguageCode };
                tenant.LanguageCode = request.LanguageCode;
            }

            if (request.Country != null && tenant.Country != request.Country)
            {
                changes["Country"] = new { From = tenant.Country, To = request.Country };
                tenant.Country = request.Country;
            }

            if (request.Region != null && tenant.Region != request.Region)
            {
                changes["Region"] = new { From = tenant.Region, To = request.Region };
                tenant.Region = request.Region;
            }

            if (request.Industry != null && tenant.Industry != request.Industry)
            {
                changes["Industry"] = new { From = tenant.Industry, To = request.Industry };
                tenant.Industry = request.Industry;
            }

            if (request.SubscriptionPlan != null && tenant.SubscriptionPlan != request.SubscriptionPlan)
            {
                changes["SubscriptionPlan"] = new { From = tenant.SubscriptionPlan, To = request.SubscriptionPlan };
                tenant.SubscriptionPlan = request.SubscriptionPlan;
            }

            // Update limits
            if (request.MaxUsers.HasValue && tenant.MaxUsers != request.MaxUsers.Value)
            {
                changes["MaxUsers"] = new { From = tenant.MaxUsers, To = request.MaxUsers.Value };
                tenant.MaxUsers = request.MaxUsers.Value;
            }

            if (request.MaxBranches.HasValue && tenant.MaxBranches != request.MaxBranches.Value)
            {
                changes["MaxBranches"] = new { From = tenant.MaxBranches, To = request.MaxBranches.Value };
                tenant.MaxBranches = request.MaxBranches.Value;
            }

            if (request.MaxRooms.HasValue && tenant.MaxRooms != request.MaxRooms.Value)
            {
                changes["MaxRooms"] = new { From = tenant.MaxRooms, To = request.MaxRooms.Value };
                tenant.MaxRooms = request.MaxRooms.Value;
            }

            if (request.MaxReservations.HasValue && tenant.MaxReservations != request.MaxReservations.Value)
            {
                changes["MaxReservations"] = new { From = tenant.MaxReservations, To = request.MaxReservations.Value };
                tenant.MaxReservations = request.MaxReservations.Value;
            }

            tenant.LastModified = DateTimeOffset.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            // Log the action
            await _auditService.LogActionAsync(
                "UpdateTenant",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Updated tenant '{tenant.Name}'",
                JsonSerializer.Serialize(changes));

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while updating the tenant", 500);
        }
    }

    public async Task<Result<bool>> LockTenantAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            tenant.IsActive = false;
            tenant.LastModified = DateTimeOffset.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "LockTenant",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Locked tenant '{tenant.Name}'. Reason: {reason}");

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while locking the tenant", 500);
        }
    }

    public async Task<Result<bool>> UnlockTenantAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            tenant.IsActive = true;
            tenant.LastModified = DateTimeOffset.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "UnlockTenant",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Unlocked tenant '{tenant.Name}'. Reason: {reason}");

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while unlocking the tenant", 500);
        }
    }

    public async Task<Result<bool>> SetTenantModeAsync(Guid tenantId, TenantMode mode, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            // This would require additional fields in the Tenant entity
            // For now, we'll use IsActive as a simple implementation
            tenant.IsActive = mode == TenantMode.Active;
            tenant.LastModified = DateTimeOffset.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "SetTenantMode",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Set tenant '{tenant.Name}' mode to {mode}. Reason: {reason}");

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tenant mode for {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while setting the tenant mode", 500);
        }
    }

    public async Task<Result<bool>> TerminateTenantAsync(Guid tenantId, string reason, DateTime? effectiveDate = null)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            // Mark for termination
            tenant.IsActive = false;
            tenant.LicenseStatus = LicenseStatus.Cancelled;
            tenant.LastModified = DateTimeOffset.UtcNow;
            tenant.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "TerminateTenant",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Terminated tenant '{tenant.Name}'. Reason: {reason}. Effective: {effectiveDate}");

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while terminating the tenant", 500);
        }
    }

    public async Task<Result<ExportJobResult>> ExportTenantDataAsync(Guid tenantId, ExportOptions options)
    {
        try
        {
            // This would typically queue a background job
            var jobId = Guid.NewGuid().ToString();

            await _auditService.LogActionAsync(
                "ExportTenantData",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Started export job {jobId} for tenant {tenantId}");

            var export = new ExportJobResult
            {
                Success = true,
                JobId = jobId,
                EstimatedCompletion = DateTime.UtcNow.AddHours(1)
            };

            return Result<ExportJobResult>.Success(export, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting export for tenant {TenantId}", tenantId);
            return Result<ExportJobResult>.Failure("An error occurred while starting the export", 500);
        }
    }

    public async Task<Result<bool>> PurgeTenantDataAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found", 404);

            // This is a destructive action - in production, this should be queued
            // and require additional approval

            await _auditService.LogActionAsync(
                "PurgeTenantData",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Purged all data for tenant '{tenant.Name}'. Reason: {reason}");

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging data for tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while purging the tenant", 500);
        }
    }

    public async Task<Result<TenantUsage>> GetTenantUsageAsync(Guid tenantId)
    {
        try
        {

            var userCount = await _context.Users.CountAsync(u => u.TenantId == tenantId);
            var branchCount = await _context.Branches.CountAsync(b => b.TenantId == tenantId);
            var roomCount = await _context.Rooms.CountAsync(r => r.TenantId == tenantId);
            var reservationCount = await _context.Reservations.CountAsync(r => r.TenantId == tenantId);
            // var activeReservations = await _context.Reservations.CountAsync(r => r.TenantId == tenantId && r.Status == ReservationStatus.Confirmed);
            // var storageUsedBytes = await _context.Files.SumAsync(f => f.Size);
            // var apiCallsLast24h = await _context.ApiLogs.CountAsync(l => l.TenantId == tenantId && l.Created >= DateTime.UtcNow.AddHours(-24));
            // var apiCallsLast7d = await _context.ApiLogs.CountAsync(l => l.TenantId == tenantId && l.Created >= DateTime.UtcNow.AddDays(-7));
            // var apiCallsLast30d = await _context.ApiLogs.CountAsync(l => l.TenantId == tenantId && l.Created >= DateTime.UtcNow.AddDays(-30));
            // var lastActivity = await _context.Users.Where(u => u.TenantId == tenantId).MaxAsync(u => u.LastLogin);

            var usage = new TenantUsage
            {
                UserCount = userCount,
                BranchCount = branchCount,
                RoomCount = roomCount,
                ReservationCount = reservationCount,
                ActiveReservations = 0,
                StorageUsedBytes = 0,
                ApiCallsLast24h = 0,
                ApiCallsLast7d = 0,
                ApiCallsLast30d = 0,
                LastActivity = DateTime.UtcNow
            };
            return Result<TenantUsage>.Success(usage, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage for tenant {TenantId}", tenantId);
            return Result<TenantUsage>.Failure("An error occurred while getting the tenant usage", 500);
        }
    }

    public Task<Result<TenantHealth>> GetTenantHealthAsync(Guid tenantId)
    {
        try
        {
            // This would typically check various health indicators
            var health = new TenantHealth
            {
                IsHealthy = true,
                Status = "Healthy",
                LastHeartbeat = DateTime.UtcNow,
                ErrorRate = 0.0,
                ErrorCountLast24h = 0,
                Issues = Array.Empty<string>(),
                CheckedAt = DateTime.UtcNow
            };
            return Task.FromResult(Result<TenantHealth>.Success(health, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health for tenant {TenantId}", tenantId);
            return Task.FromResult(Result<TenantHealth>.Failure("An error occurred while getting the tenant health", 500));
        }
    }

    public async Task<Result<Domain.Entities.SuperAdmin.Plan>> CreatePlanAsync(CreatePlanRequest request)
    {
        try
        {
            var existingPlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Name == request.Name);

            if (existingPlan != null)
                return Result<Domain.Entities.SuperAdmin.Plan>.Failure($"Plan with name '{request.Name}' already exists", 400);

            var plan = _mapper.Map<Domain.Entities.SuperAdmin.Plan>(request);
            plan.Id = Guid.NewGuid();
            plan.CreatedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;

            _context.Plans.Add(plan);

            // Create plan features
            foreach (var featureRequest in request.Features)
            {
                var feature = new Domain.Entities.SuperAdmin.PlanFeature
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    Name = featureRequest.Name,
                    Description = featureRequest.Description,
                    Included = featureRequest.Included,
                    Limit = featureRequest.Limit,
                    Unit = featureRequest.Unit,
                    Plan = plan
                };
                _context.PlanFeatures.Add(feature);
            }

            // Create plan limits
            var limits = new Domain.Entities.SuperAdmin.PlanLimits
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                MaxUsers = request.Limits.MaxUsers,
                MaxBranches = request.Limits.MaxBranches,
                MaxRooms = request.Limits.MaxRooms,
                MaxReservations = request.Limits.MaxReservations,
                MaxStorageGB = request.Limits.MaxStorageGB,
                ApiRateLimit = request.Limits.ApiRateLimit,
                SupportLevel = request.Limits.SupportLevel,
                SLA = request.Limits.SLA,
                Plan = plan
            };
            _context.PlanLimits.Add(limits);

            // Store modules as JSON
            if (request.Modules.Length > 0)
            {
                plan.Modules = System.Text.Json.JsonSerializer.Serialize(request.Modules);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "CreatePlan",
                "Plan",
                plan.Id.ToString(),
                plan.Id,
                $"Created plan '{request.Name}' with {request.Features.Length} features",
                System.Text.Json.JsonSerializer.Serialize(request));

            return Result<Domain.Entities.SuperAdmin.Plan>.Success(plan, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plan: {PlanName}", request.Name);
            return Result<Domain.Entities.SuperAdmin.Plan>.Failure("An error occurred while creating the plan", 500);
        }
    }
}
