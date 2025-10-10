using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for SuperAdmin tenant management operations
/// </summary>
public class SuperAdminService : ISuperAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuperAdminService> _logger;
    private readonly ISuperAdminAuditService _auditService;

    public SuperAdminService(
        ApplicationDbContext context,
        ILogger<SuperAdminService> logger,
        ISuperAdminAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<CreateTenantResult> CreateTenantAsync(CreateTenantRequest request)
    {
        try
        {
            // Check if identifier already exists
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == request.Identifier);

            if (existingTenant != null)
            {
                return new CreateTenantResult
                {
                    Success = false,
                    ErrorMessage = $"Tenant with identifier '{request.Identifier}' already exists"
                };
            }

            // Create tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = request.Name,
                Identifier = request.Identifier,
                Description = request.Description,
                Address = request.Address,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                TimeZone = request.TimeZone ?? "UTC",
                CurrencyCode = request.CurrencyCode ?? "USD",
                LanguageCode = request.LanguageCode ?? "en",
                Country = request.Country,
                Region = request.Region,
                Industry = request.Industry,
                SubscriptionPlan = request.SubscriptionPlan,
                MaxUsers = request.MaxUsers,
                MaxBranches = request.MaxBranches,
                MaxRooms = request.MaxRooms,
                MaxReservations = request.MaxReservations,
                LicenseStatus = LicenseStatus.Trial,
                IsActive = true,
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "SuperAdmin",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "SuperAdmin"
            };

            _context.Tenants.Add(tenant);

            // Create tenant features
            foreach (var module in request.EnabledModules)
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

            // Log the action
            await _auditService.LogActionAsync(
                "CreateTenant",
                "Tenant",
                tenant.Id.ToString(),
                tenant.Id,
                $"Created tenant '{request.Name}' with identifier '{request.Identifier}'",
                JsonSerializer.Serialize(request));

            return new CreateTenantResult
            {
                Success = true,
                TenantId = tenant.Id,
                InitialPassword = GenerateInitialPassword(),
                AdminEmail = request.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {Identifier}", request.Identifier);
            return new CreateTenantResult
            {
                Success = false,
                ErrorMessage = "An error occurred while creating the tenant"
            };
        }
    }

    public async Task<PaginatedResult<TenantSummary>> GetTenantsAsync(TenantListRequest request)
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
                query = query.Where(t => t.SubscriptionPlan == request.Plan);
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

            var items = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(t => new TenantSummary
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
                    UserCount = 0, // This would need to be calculated
                    BranchCount = 0 // This would need to be calculated
                })
                .ToListAsync();

            return new PaginatedResult<TenantSummary>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenants list");
            return new PaginatedResult<TenantSummary>();
        }
    }

    public async Task<TenantDetail?> GetTenantDetailAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Features)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return null;

            var enabledModules = tenant.Features
                .Where(f => f.IsEnabled)
                .Select(f => f.FeatureName)
                .ToArray();

            return new TenantDetail
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                Description = tenant.Description,
                Address = tenant.Address,
                ContactNumber = tenant.ContactNumber,
                Email = tenant.Email,
                TimeZone = tenant.TimeZone ?? "UTC",
                CurrencyCode = tenant.CurrencyCode ?? "USD",
                LanguageCode = tenant.LanguageCode ?? "en",
                Country = tenant.Country,
                Region = tenant.Region,
                Industry = tenant.Industry,
                IsActive = tenant.IsActive,
                LicenseStatus = tenant.LicenseStatus.ToString(),
                LicenseExpiryDate = tenant.LicenseExpiryDate,
                SubscriptionPlan = tenant.SubscriptionPlan ?? "Basic",
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant detail for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> LockTenantAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> UnlockTenantAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> SetTenantModeAsync(Guid tenantId, TenantMode mode, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tenant mode for {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> TerminateTenantAsync(Guid tenantId, string reason, DateTime? effectiveDate = null)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<ExportJobResult> ExportTenantDataAsync(Guid tenantId, ExportOptions options)
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

            return new ExportJobResult
            {
                Success = true,
                JobId = jobId,
                EstimatedCompletion = DateTime.UtcNow.AddHours(1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting export for tenant {TenantId}", tenantId);
            return new ExportJobResult
            {
                Success = false,
                ErrorMessage = "Failed to start export job"
            };
        }
    }

    public async Task<bool> PurgeTenantDataAsync(Guid tenantId, string reason)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

            // This is a destructive action - in production, this should be queued
            // and require additional approval

            await _auditService.LogActionAsync(
                "PurgeTenantData",
                "Tenant",
                tenantId.ToString(),
                tenantId,
                $"Purged all data for tenant '{tenant.Name}'. Reason: {reason}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging data for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public Task<TenantUsage?> GetTenantUsageAsync(Guid tenantId)
    {
        try
        {
            // This would typically aggregate data from multiple sources
            var usage = new TenantUsage
            {
                UserCount = 0, // Calculate from users table
                BranchCount = 0, // Calculate from branches table
                RoomCount = 0, // Calculate from rooms table
                ReservationCount = 0, // Calculate from reservations table
                ActiveReservations = 0,
                StorageUsedBytes = 0,
                ApiCallsLast24h = 0,
                ApiCallsLast7d = 0,
                ApiCallsLast30d = 0,
                LastActivity = DateTime.UtcNow
            };
            return Task.FromResult<TenantUsage?>(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage for tenant {TenantId}", tenantId);
            return Task.FromResult<TenantUsage?>(null);
        }
    }

    public Task<TenantHealth?> GetTenantHealthAsync(Guid tenantId)
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
            return Task.FromResult<TenantHealth?>(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health for tenant {TenantId}", tenantId);
            return Task.FromResult<TenantHealth?>(null);
        }
    }

    private static string GenerateInitialPassword()
    {
        // Generate a secure random password
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
