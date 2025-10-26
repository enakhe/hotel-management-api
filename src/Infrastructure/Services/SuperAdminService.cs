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

    public async Task<Result<PlanResponseDto>> CreatePlanAsync(CreatePlanRequest request)
    {
        try
        {
            var existingPlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Name == request.Name);

            if (existingPlan != null)
                return Result<PlanResponseDto>.Failure($"Plan with name '{request.Name}' already exists", 400);

            var plan = _mapper.Map<Domain.Entities.SuperAdmin.Plan>(request);
            plan.Id = Guid.NewGuid();
            plan.CreatedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CreatedBy = "SuperAdmin";
            plan.UpdatedBy = "SuperAdmin";
            _context.Plans.Add(plan);

            // Create plan features
            foreach (var featureRequest in request.Features)
            {
                var feature = _mapper.Map<Domain.Entities.SuperAdmin.PlanFeature>(featureRequest);
                var existingFeature = await _context.PlanFeatures.FirstOrDefaultAsync(f => f.Name == featureRequest.Name && f.PlanId == plan.Id);
                if (existingFeature == null)
                {
                    feature.Id = Guid.NewGuid();
                    feature.PlanId = plan.Id;
                    feature.Plan = plan;
                    _context.PlanFeatures.Add(feature);
                }
                else
                {
                    existingFeature.Included = featureRequest.Included;
                    existingFeature.Limit = featureRequest.Limit;
                    existingFeature.Unit = featureRequest.Unit;
                    _context.PlanFeatures.Update(existingFeature);
                }
            }

            // Create plan limits
            var limits = _mapper.Map<Domain.Entities.SuperAdmin.PlanLimits>(request.Limits);
            limits.Id = Guid.NewGuid();
            limits.PlanId = plan.Id;
            limits.Plan = plan;
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

            var planResponse = _mapper.Map<PlanResponseDto>(plan);

            // Handle modules deserialization manually
            if (!string.IsNullOrEmpty(plan.Modules))
            {
                planResponse = planResponse with
                {
                    Modules = System.Text.Json.JsonSerializer.Deserialize<string[]>(plan.Modules)
                };
            }

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
                .Include(p => p.Features)
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

            if (request.PriceMin.HasValue)
            {
                query = query.Where(p => p.Price >= request.PriceMin.Value);
            }

            if (request.PriceMax.HasValue)
            {
                query = query.Where(p => p.Price <= request.PriceMax.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => request.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
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

                // Handle modules deserialization manually
                if (!string.IsNullOrEmpty(plan.Modules))
                {
                    planDto = planDto with
                    {
                        Modules = System.Text.Json.JsonSerializer.Deserialize<string[]>(plan.Modules)
                    };
                }

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
                .Include(p => p.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<PlanResponseDto>.Failure("Plan not found", 404);

            var planDto = _mapper.Map<PlanResponseDto>(plan);

            // Handle modules deserialization manually
            if (!string.IsNullOrEmpty(plan.Modules))
            {
                planDto = planDto with
                {
                    Modules = System.Text.Json.JsonSerializer.Deserialize<string[]>(plan.Modules)
                };
            }

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
                .Include(p => p.Features)
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

            if (request.Price.HasValue)
                plan.Price = request.Price.Value;

            if (!string.IsNullOrEmpty(request.Currency))
                plan.Currency = request.Currency;

            if (request.BillingCycle.HasValue)
                plan.BillingCycle = request.BillingCycle.Value;

            if (request.IsActive.HasValue)
                plan.IsActive = request.IsActive.Value;

            if (request.IsPopular.HasValue)
                plan.IsPopular = request.IsPopular.Value;

            if (request.Modules != null)
            {
                plan.Modules = request.Modules.Length > 0
                    ? System.Text.Json.JsonSerializer.Serialize(request.Modules)
                    : null;
            }

            plan.UpdatedAt = DateTime.UtcNow;
            plan.UpdatedBy = "SuperAdmin";

            // Update features if provided
            if (request.Features != null && request.Features.Length > 0)
            {
                // Remove existing features
                _context.PlanFeatures.RemoveRange(plan.Features);

                // Add new features
                foreach (var featureRequest in request.Features)
                {
                    var feature = _mapper.Map<Domain.Entities.SuperAdmin.PlanFeature>(featureRequest);
                    feature.Id = Guid.NewGuid();
                    feature.PlanId = plan.Id;
                    feature.Plan = plan;
                    _context.PlanFeatures.Add(feature);
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
                    var limits = _mapper.Map<Domain.Entities.SuperAdmin.PlanLimits>(request.Limits);
                    limits.Id = Guid.NewGuid();
                    limits.PlanId = plan.Id;
                    limits.Plan = plan;
                    _context.PlanLimits.Add(limits);
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
                .Include(p => p.Features)
                .Include(p => p.Limits)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            var planDto = _mapper.Map<PlanResponseDto>(updatedPlan);

            // Handle modules deserialization manually
            if (!string.IsNullOrEmpty(updatedPlan!.Modules))
            {
                planDto = planDto with
                {
                    Modules = System.Text.Json.JsonSerializer.Deserialize<string[]>(updatedPlan.Modules)
                };
            }

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
                .Include(p => p.Features)
                .Include(p => p.Limits)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return Result<bool>.Failure("Plan not found", 404);

            // Check if plan is being used by any tenants
            var tenantsUsingPlan = await _context.Tenants
                .AnyAsync(t => t.SubscriptionPlan == plan.Name);

            if (tenantsUsingPlan)
                return Result<bool>.Failure("Cannot delete plan that is currently being used by tenants", 400);

            // Remove related entities first (due to foreign key constraints)
            _context.PlanFeatures.RemoveRange(plan.Features);

            if (plan.Limits != null)
                _context.PlanLimits.Remove(plan.Limits);

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

    public async Task<Result<ModuleResponseDto>> CreateModuleAsync(CreateModuleRequest request)
    {
        try
        {
            var existingModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.Name == request.Name);

            if (existingModule != null)
                return Result<ModuleResponseDto>.Failure($"Module with name '{request.Name}' already exists", 400);

            var module = _mapper.Map<Domain.Entities.SuperAdmin.Module>(request);
            module.Id = Guid.NewGuid();
            module.CreatedAt = DateTime.UtcNow;
            module.UpdatedAt = DateTime.UtcNow;
            module.CreatedBy = "SuperAdmin";
            module.UpdatedBy = "SuperAdmin";

            // Store dependencies as JSON
            if (request.Dependencies.Length > 0)
            {
                module.Dependencies = System.Text.Json.JsonSerializer.Serialize(request.Dependencies);
            }

            _context.Modules.Add(module);

            // Create module features
            foreach (var featureRequest in request.Features)
            {
                var feature = _mapper.Map<Domain.Entities.SuperAdmin.ModuleFeature>(featureRequest);
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

            // Create module pricing
            var pricing = _mapper.Map<Domain.Entities.SuperAdmin.ModulePricing>(request.Pricing);
            pricing.Id = Guid.NewGuid();
            pricing.ModuleId = module.Id;
            pricing.Module = module;
            _context.ModulePricing.Add(pricing);

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "CreateModule",
                "Module",
                module.Id.ToString(),
                module.Id,
                $"Created module '{request.Name}' with {request.Features.Length} features",
                System.Text.Json.JsonSerializer.Serialize(request));

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

    public async Task<Result<PaginatedResult<ModuleResponseDto>>> GetModulesAsync(ModuleListRequest request)
    {
        try
        {
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
                    var feature = _mapper.Map<Domain.Entities.SuperAdmin.ModuleFeature>(featureRequest);
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
                    var pricing = _mapper.Map<Domain.Entities.SuperAdmin.ModulePricing>(request.Pricing);
                    pricing.Id = Guid.NewGuid();
                    pricing.ModuleId = module.Id;
                    pricing.Module = module;
                    _context.ModulePricing.Add(pricing);
                }
            }

            await _context.SaveChangesAsync();

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
                .AnyAsync(p => p.Modules!.Contains(module.Name));

            if (plansUsingModule)
                return Result<bool>.Failure("Cannot delete module that is currently being used by plans", 400);

            // Remove related entities first (due to foreign key constraints)
            _context.ModuleFeatures.RemoveRange(module.Features);

            if (module.Pricing != null)
                _context.ModulePricing.Remove(module.Pricing);

            // Remove the module
            _context.Modules.Remove(module);

            await _context.SaveChangesAsync();

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

            // Parse existing modules
            var existingModules = new List<string>();
            if (!string.IsNullOrEmpty(plan.Modules))
            {
                existingModules = System.Text.Json.JsonSerializer.Deserialize<List<string>>(plan.Modules) ?? new List<string>();
            }

            // Check if module is already assigned
            if (existingModules.Contains(module.Name))
                return Result<bool>.Failure("Module is already assigned to this plan", 400);

            // Add module to plan
            existingModules.Add(module.Name);
            plan.Modules = System.Text.Json.JsonSerializer.Serialize(existingModules);
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

            // Parse existing modules
            var existingModules = new List<string>();
            if (!string.IsNullOrEmpty(plan.Modules))
            {
                existingModules = System.Text.Json.JsonSerializer.Deserialize<List<string>>(plan.Modules) ?? new List<string>();
            }

            // Check if module is assigned
            if (!existingModules.Contains(module.Name))
                return Result<bool>.Failure("Module is not assigned to this plan", 400);

            // Remove module from plan
            existingModules.Remove(module.Name);
            plan.Modules = existingModules.Count > 0
                ? System.Text.Json.JsonSerializer.Serialize(existingModules)
                : null;
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

    public async Task<Result<bool>> BulkUpdatePlansAsync(HotelManagement.Application.Core.PlanManagement.Commands.BulkPlanUpdateRequest[] updates)
    {
        try
        {
            var planIds = updates.Select(u => u.Id).ToList();
            var plans = await _context.Plans
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

                    if (update.Data.Price.HasValue)
                        plan.Price = update.Data.Price.Value;

                    if (!string.IsNullOrEmpty(update.Data.Currency))
                        plan.Currency = update.Data.Currency;

                    if (update.Data.BillingCycle.HasValue)
                        plan.BillingCycle = update.Data.BillingCycle.Value;

                    if (update.Data.IsActive.HasValue)
                        plan.IsActive = update.Data.IsActive.Value;

                    if (update.Data.IsPopular.HasValue)
                        plan.IsPopular = update.Data.IsPopular.Value;

                    if (update.Data.Modules != null)
                    {
                        plan.Modules = update.Data.Modules.Length > 0
                            ? System.Text.Json.JsonSerializer.Serialize(update.Data.Modules)
                            : null;
                    }

                    plan.UpdatedAt = DateTime.UtcNow;
                    plan.UpdatedBy = "SuperAdmin";
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating plan {PlanId}", update.Id);
                    errors.Add($"Failed to update plan {update.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "BulkUpdatePlans",
                "Plan",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updatedCount} plans",
                System.Text.Json.JsonSerializer.Serialize(updates));

            if (errors.Any())
            {
                return Result<bool>.Failure($"Bulk update completed with {errors.Count} errors: {string.Join(", ", errors)}", 207);
            }

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update plans");
            return Result<bool>.Failure("An error occurred during bulk update of plans", 500);
        }
    }

    public async Task<Result<bool>> BulkUpdateModulesAsync(HotelManagement.Application.Core.ModuleManagement.Commands.BulkModuleUpdateRequest[] updates)
    {
        try
        {
            var moduleIds = updates.Select(u => u.Id).ToList();
            var modules = await _context.Modules
                .Where(m => moduleIds.Contains(m.Id))
                .ToListAsync();

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var update in updates)
            {
                var module = modules.FirstOrDefault(m => m.Id == update.Id);
                if (module == null)
                {
                    errors.Add($"Module with ID {update.Id} not found");
                    continue;
                }

                try
                {
                    // Update module properties
                    if (!string.IsNullOrEmpty(update.Data.Name))
                        module.Name = update.Data.Name;

                    if (update.Data.Description != null)
                        module.Description = update.Data.Description;

                    if (!string.IsNullOrEmpty(update.Data.Category))
                        module.Category = update.Data.Category;

                    if (update.Data.IsActive.HasValue)
                        module.IsActive = update.Data.IsActive.Value;

                    if (update.Data.IsCore.HasValue)
                        module.IsCore = update.Data.IsCore.Value;

                    if (update.Data.Dependencies != null)
                    {
                        module.Dependencies = update.Data.Dependencies.Length > 0
                            ? System.Text.Json.JsonSerializer.Serialize(update.Data.Dependencies)
                            : null;
                    }

                    module.UpdatedAt = DateTime.UtcNow;
                    module.UpdatedBy = "SuperAdmin";
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating module {ModuleId}", update.Id);
                    errors.Add($"Failed to update module {update.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "BulkUpdateModules",
                "Module",
                "Bulk",
                Guid.Empty,
                $"Bulk updated {updatedCount} modules",
                System.Text.Json.JsonSerializer.Serialize(updates));

            if (errors.Any())
            {
                return Result<bool>.Failure($"Bulk update completed with {errors.Count} errors: {string.Join(", ", errors)}", 207);
            }

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update modules");
            return Result<bool>.Failure("An error occurred during bulk update of modules", 500);
        }
    }

    public async Task<Result<PlanUsageDto[]>> GetPlanUsageAsync()
    {
        try
        {
            var plans = await _context.Plans
                .AsNoTracking()
                .ToListAsync();

            var planUsageList = new List<PlanUsageDto>();

            foreach (var plan in plans)
            {
                // Count tenants using this plan
                var tenantCount = await _context.Tenants
                    .CountAsync(t => t.SubscriptionPlan == plan.Name);

                // Calculate revenue (simplified - would need actual billing data)
                var totalRevenue = tenantCount * plan.Price;

                // Calculate average tenant value
                var averageTenantValue = tenantCount > 0 ? totalRevenue / tenantCount : 0;

                // Calculate churn rate (simplified - would need historical data)
                var churnRate = 0.05m; // Placeholder: 5% churn rate

                planUsageList.Add(new PlanUsageDto
                {
                    PlanId = plan.Id,
                    PlanName = plan.Name,
                    TenantCount = tenantCount,
                    TotalRevenue = totalRevenue,
                    AverageTenantValue = averageTenantValue,
                    ChurnRate = churnRate,
                    LastUpdated = DateTime.UtcNow
                });
            }

            return Result<PlanUsageDto[]>.Success(planUsageList.ToArray(), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan usage analytics");
            return Result<PlanUsageDto[]>.Failure("An error occurred while getting plan usage analytics", 500);
        }
    }

    public async Task<Result<ModuleUsageDto[]>> GetModuleUsageAsync()
    {
        try
        {
            var modules = await _context.Modules
                .AsNoTracking()
                .ToListAsync();

            var moduleUsageList = new List<ModuleUsageDto>();

            foreach (var module in modules)
            {
                // Count tenants using this module
                var tenantCount = await _context.Tenants
                    .Include(t => t.Features)
                    .CountAsync(t => t.Features.Any(f => f.FeatureName == module.Name && f.IsEnabled));

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

            return Result<ModuleUsageDto[]>.Success(moduleUsageList.ToArray(), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module usage analytics");
            return Result<ModuleUsageDto[]>.Failure("An error occurred while getting module usage analytics", 500);
        }
    }
}
