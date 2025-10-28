using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing licenses and license operations
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LicenseService> _logger;

    public LicenseService(ApplicationDbContext context, ILogger<LicenseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CRUD Operations
    public async Task<Result<LicenseResponseDto>> CreateLicenseAsync(CreateLicenseRequest request)
    {
        try
        {
            // Validate that the Plan exists
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Features)
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .Include(p => p.Limits)
                .FirstOrDefaultAsync(p => p.Id == request.PlanId);

            if (plan == null)
                return Result<LicenseResponseDto>.Failure($"Plan with ID '{request.PlanId}' not found", 400);

            // Generate license key if not provided
            var licenseKey = request.CustomLicenseKey ?? Guid.NewGuid().ToString("N")[..16].ToUpper();

            // Check if license key is unique
            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);

            if (existingLicense != null)
                return Result<LicenseResponseDto>.Failure("License key already exists", 400);

            // Create license entity
            var license = new Domain.Entities.License
            {
                Id = Guid.NewGuid(),
                PlanId = request.PlanId,
                LicenseKey = licenseKey,
                Type = request.Type,
                Status = LicenseStatusType.Active,
                ExpirationDate = request.ExpirationDate,
                MaxValidations = request.MaxValidations,
                HardwareId = request.HardwareId,
                DomainRestrictions = request.DomainRestrictions != null ? string.Join(",", request.DomainRestrictions) : null,
                IpRestrictions = request.IpRestrictions != null ? string.Join(",", request.IpRestrictions) : null,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                IssuedDate = DateTime.UtcNow,
                CreatedBy = "SuperAdmin"
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Collect all plan features from modules
            var planFeatures = new List<ModuleFeatureResponseDto>();
            var planModules = new List<ModulePricingResponseDto>();

            foreach (var planModule in plan.PlanModules)
            {
                if (planModule.Module != null)
                {
                    // Add module features
                    foreach (var feature in planModule.Module.Features)
                    {
                        var featureDto = new ModuleFeatureResponseDto
                        {
                            Id = feature.Id,
                            Name = feature.Name,
                            Description = feature.Description,
                            IsEnabled = feature.IsEnabled,
                            Configuration = !string.IsNullOrEmpty(feature.Configuration)
                                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(feature.Configuration)
                                : null
                        };
                        planFeatures.Add(featureDto);
                    }

                    // Add module pricing
                    if (planModule.Module.Pricing != null)
                    {
                        var pricingDto = new ModulePricingResponseDto
                        {
                            Id = planModule.Module.Pricing.Id,
                            Type = planModule.Module.Pricing.Type,
                            Price = planModule.Module.Pricing.Price,
                            Currency = planModule.Module.Pricing.Currency,
                            BillingCycle = planModule.Module.Pricing.BillingCycle,
                            MinQuantity = planModule.Module.Pricing.MinQuantity,
                            MaxQuantity = planModule.Module.Pricing.MaxQuantity
                        };
                        planModules.Add(pricingDto);
                    }
                }
            }

            // Map plan limits
            var planLimits = plan.Limits != null ? new LimitsResponseDto
            {
                Id = plan.Limits.Id,
                Name = plan.Limits.Name,
                Description = plan.Limits.Description,
                MaxUsers = plan.Limits.MaxUsers,
                MaxBranches = plan.Limits.MaxBranches,
                MaxRooms = plan.Limits.MaxRooms,
                MaxReservations = plan.Limits.MaxReservations,
                MaxStorageGB = plan.Limits.MaxStorageGB,
                ApiRateLimit = plan.Limits.ApiRateLimit,
                ConcurrentSessions = plan.Limits.ConcurrentSessions,
                MaxGuests = plan.Limits.MaxGuests,
                MaxBookings = plan.Limits.MaxBookings,
                MaxReports = plan.Limits.MaxReports,
                MaxIntegrations = plan.Limits.MaxIntegrations,
                CustomLimits = !string.IsNullOrEmpty(plan.Limits.CustomLimits)
                    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(plan.Limits.CustomLimits)
                    : null,
                IsDefault = plan.Limits.IsDefault,
                IsActive = plan.Limits.IsActive,
                CreatedAt = DateTime.UtcNow, // Use current time as fallback
                UpdatedAt = DateTime.UtcNow, // Use current time as fallback
                CreatedBy = plan.Limits.CreatedBy,
                LastModifiedBy = plan.Limits.LastModifiedBy
            } : null;

            // Map to response DTO
            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = plan.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = request.DomainRestrictions,
                IpRestrictions = request.IpRestrictions,
                PlanFeatures = planFeatures.ToArray(),
                PlanModules = planModules.ToArray(),
                PlanLimits = planLimits,
                Metadata = request.Metadata,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.IssuedDate,
                LastModifiedBy = license.CreatedBy
            };

            return Result<LicenseResponseDto>.Success(response, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating license");
            return Result<LicenseResponseDto>.Failure("An error occurred while creating the license", 500);
        }
    }

    public async Task<Result<PaginatedResult<LicenseResponseDto>>> GetLicensesAsync(LicenseListRequest request)
    {
        try
        {
            var query = _context.Licenses
                .Include(l => l.Plan)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(l => l.LicenseKey.Contains(request.Query) ||
                                        l.Plan!.Name.Contains(request.Query));
            }

            if (request.Status.HasValue)
                query = query.Where(l => l.Status == request.Status.Value);

            if (request.Type.HasValue)
                query = query.Where(l => l.Type == request.Type.Value);

            if (request.PlanId.HasValue)
                query = query.Where(l => l.PlanId == request.PlanId.Value);

            if (request.Expired.HasValue)
            {
                if (request.Expired.Value)
                    query = query.Where(l => l.ExpirationDate < DateTime.UtcNow);
                else
                    query = query.Where(l => l.ExpirationDate >= DateTime.UtcNow);
            }

            if (request.ExpiresInDays.HasValue)
            {
                var expiryDate = DateTime.UtcNow.AddDays(request.ExpiresInDays.Value);
                query = query.Where(l => l.ExpirationDate <= expiryDate && l.ExpirationDate >= DateTime.UtcNow);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "created" => request.SortDescending ? query.OrderByDescending(l => l.IssuedDate) : query.OrderBy(l => l.IssuedDate),
                "expiration" => request.SortDescending ? query.OrderByDescending(l => l.ExpirationDate) : query.OrderBy(l => l.ExpirationDate),
                "status" => request.SortDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
                "type" => request.SortDescending ? query.OrderByDescending(l => l.Type) : query.OrderBy(l => l.Type),
                _ => query.OrderByDescending(l => l.IssuedDate)
            };

            var totalCount = await query.CountAsync();
            var licenses = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(l => new LicenseResponseDto
                {
                    Id = l.Id,
                    LicenseKey = l.LicenseKey,
                    PlanId = l.PlanId,
                    PlanName = l.Plan!.Name,
                    Type = l.Type,
                    Status = l.Status,
                    ExpirationDate = l.ExpirationDate,
                    MaxValidations = l.MaxValidations,
                    ValidationCount = l.ValidationCount,
                    HardwareId = l.HardwareId,
                    IssuedDate = l.IssuedDate,
                    CreatedBy = l.CreatedBy,
                })
                .ToListAsync();

            var paginatedResult = new PaginatedResult<LicenseResponseDto>
            {
                Items = licenses,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size,
            };

            return Result<PaginatedResult<LicenseResponseDto>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting licenses");
            return Result<PaginatedResult<LicenseResponseDto>>.Failure("An error occurred while getting licenses", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> GetLicenseByIdAsync(Guid licenseId)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = license.Metadata != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) : null,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by ID {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while getting the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> GetLicenseByKeyAsync(string licenseKey)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = license.Metadata != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) : null,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by key {LicenseKey}", licenseKey);
            return Result<LicenseResponseDto>.Failure("An error occurred while getting the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> UpdateLicenseAsync(Guid licenseId, UpdateLicenseRequest request)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            // Update license properties
            if (request.Status.HasValue)
                license.Status = request.Status.Value;

            if (request.ExpirationDate.HasValue)
                license.ExpirationDate = request.ExpirationDate.Value;

            if (request.MaxValidations.HasValue)
                license.MaxValidations = request.MaxValidations.Value;

            if (request.HardwareId != null)
                license.HardwareId = request.HardwareId;

            if (request.DomainRestrictions != null)
                license.DomainRestrictions = string.Join(",", request.DomainRestrictions);

            if (request.IpRestrictions != null)
                license.IpRestrictions = string.Join(",", request.IpRestrictions);

            if (request.Metadata != null)
                license.Metadata = System.Text.Json.JsonSerializer.Serialize(request.Metadata);

            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = license.Metadata != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) : null,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while updating the license", 500);
        }
    }

    public async Task<Result<bool>> DeleteLicenseAsync(Guid licenseId)
    {
        try
        {
            var license = await _context.Licenses.FindAsync(licenseId);

            if (license == null)
                return Result<bool>.Failure("License not found", 404);

            // Check if license is in use by any tenant
            var tenantUsingLicense = await _context.Tenants
                .AnyAsync(t => t.LicenseId == licenseId);

            if (tenantUsingLicense)
                return Result<bool>.Failure("Cannot delete license that is currently in use by a tenant", 400);

            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting license {LicenseId}", licenseId);
            return Result<bool>.Failure("An error occurred while deleting the license", 500);
        }
    }

    // License Management Operations
    public async Task<Result<LicenseResponseDto>> RenewLicenseAsync(Guid licenseId, LicenseRenewalRequest request)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            // Update expiration date
            license.ExpirationDate = request.NewExpirationDate;
            license.Status = LicenseStatusType.Active; // Reactivate if suspended/expired
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = license.Metadata != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) : null,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while renewing the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> TransferLicenseAsync(Guid licenseId, LicenseTransferRequest request)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            // Validate that the new tenant exists
            var newTenant = await _context.Tenants.FindAsync(request.NewTenantId);
            if (newTenant == null)
                return Result<LicenseResponseDto>.Failure($"Tenant with ID '{request.NewTenantId}' not found", 400);

            // Update license metadata with transfer information
            var metadata = license.Metadata != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>();

            metadata["TransferDate"] = request.TransferDate;
            metadata["TransferReason"] = request.Reason;
            metadata["PreviousTenantId"] = newTenant.Id;

            license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = metadata,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while transferring the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> SuspendLicenseAsync(Guid licenseId, string reason)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            license.Status = LicenseStatusType.Suspended;
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            // Add suspension reason to metadata
            var metadata = license.Metadata != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>();

            metadata["SuspensionReason"] = reason;
            metadata["SuspendedAt"] = DateTime.UtcNow;
            license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = metadata,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while suspending the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> RevokeLicenseAsync(Guid licenseId, string reason)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            license.Status = LicenseStatusType.Revoked;
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            // Add revocation reason to metadata
            var metadata = license.Metadata != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>();

            metadata["RevocationReason"] = reason;
            metadata["RevokedAt"] = DateTime.UtcNow;
            license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = metadata,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while revoking the license", 500);
        }
    }

    public async Task<Result<LicenseResponseDto>> ActivateLicenseAsync(Guid licenseId, string reason)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return Result<LicenseResponseDto>.Failure("License not found", 404);

            license.Status = LicenseStatusType.Active;
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "SuperAdmin";

            // Add activation reason to metadata
            var metadata = license.Metadata != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>();

            metadata["ActivationReason"] = reason;
            metadata["ActivatedAt"] = DateTime.UtcNow;
            license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            var response = new LicenseResponseDto
            {
                Id = license.Id,
                LicenseKey = license.LicenseKey,
                PlanId = license.PlanId,
                PlanName = license.Plan!.Name,
                Type = license.Type,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                MaxValidations = license.MaxValidations,
                ValidationCount = license.ValidationCount,
                HardwareId = license.HardwareId,
                DomainRestrictions = license.DomainRestrictions != null ? license.DomainRestrictions.Split(',') : null,
                IpRestrictions = license.IpRestrictions != null ? license.IpRestrictions.Split(',') : null,
                Metadata = metadata,
                Created = license.IssuedDate,
                CreatedBy = license.CreatedBy,
                LastModified = license.LastValidated ?? license.IssuedDate,
                LastModifiedBy = license.LastModifiedBy
            };

            return Result<LicenseResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating license {LicenseId}", licenseId);
            return Result<LicenseResponseDto>.Failure("An error occurred while activating the license", 500);
        }
    }

    // License Validation
    public async Task<Result<LicenseValidationResponse>> ValidateLicenseAsync(LicenseValidationRequest request)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Plan)
                .Include(l => l.Plan!.PlanModules)
                    .ThenInclude(pm => pm.Module)
                .Include(l => l.Plan!.PlanModules)
                    .ThenInclude(pm => pm.Module.Features)
                .Include(l => l.Plan!.Limits)
                .FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey);

            if (license == null)
                return Result<LicenseValidationResponse>.Failure("Invalid license key", 400);

            // Check if license is active
            if (license.Status != LicenseStatusType.Active)
                return Result<LicenseValidationResponse>.Failure($"License is {license.Status.ToString().ToLower()}", 400);

            // Check expiration
            if (license.ExpirationDate < DateTime.UtcNow)
                return Result<LicenseValidationResponse>.Failure("License has expired", 400);

            // Check validation count limit
            if (license.MaxValidations.HasValue && license.ValidationCount >= license.MaxValidations.Value)
                return Result<LicenseValidationResponse>.Failure("Maximum validation count exceeded", 400);

            // Check hardware ID if provided
            if (!string.IsNullOrEmpty(request.HardwareId) && !string.IsNullOrEmpty(license.HardwareId))
            {
                if (license.HardwareId != request.HardwareId)
                    return Result<LicenseValidationResponse>.Failure("Hardware ID mismatch", 400);
            }

            // Check domain restrictions if provided
            if (!string.IsNullOrEmpty(request.Domain) && !string.IsNullOrEmpty(license.DomainRestrictions))
            {
                var allowedDomains = license.DomainRestrictions.Split(',');
                if (!allowedDomains.Contains(request.Domain))
                    return Result<LicenseValidationResponse>.Failure("Domain not allowed", 400);
            }

            // Check IP restrictions if provided
            if (!string.IsNullOrEmpty(request.IpAddress) && !string.IsNullOrEmpty(license.IpRestrictions))
            {
                var allowedIps = license.IpRestrictions.Split(',');
                if (!allowedIps.Contains(request.IpAddress))
                    return Result<LicenseValidationResponse>.Failure("IP address not allowed", 400);
            }

            // Increment validation count
            license.ValidationCount++;
            license.LastValidated = DateTime.UtcNow;
            license.LastModifiedBy = "System";
            await _context.SaveChangesAsync();

            // Get enabled features
            var enabledFeatures = license.Plan!.PlanModules
                .SelectMany(pm => pm.Module.Features.Where(f => f.IsEnabled))
                .Select(f => new ModuleFeatureResponseDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description ?? string.Empty,
                    IsEnabled = f.IsEnabled,
                    Configuration = !string.IsNullOrEmpty(f.Configuration)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(f.Configuration)
                        : null
                })
                .ToArray();

            // Map plan limits
            var planLimits = license.Plan.Limits != null ? new LimitsResponseDto
            {
                Id = license.Plan.Limits.Id,
                Name = license.Plan.Limits.Name,
                Description = license.Plan.Limits.Description,
                MaxUsers = license.Plan.Limits.MaxUsers,
                MaxBranches = license.Plan.Limits.MaxBranches,
                MaxRooms = license.Plan.Limits.MaxRooms,
                MaxReservations = license.Plan.Limits.MaxReservations,
                MaxStorageGB = license.Plan.Limits.MaxStorageGB,
                ApiRateLimit = license.Plan.Limits.ApiRateLimit,
                ConcurrentSessions = license.Plan.Limits.ConcurrentSessions,
                MaxGuests = license.Plan.Limits.MaxGuests,
                MaxBookings = license.Plan.Limits.MaxBookings,
                MaxReports = license.Plan.Limits.MaxReports,
                MaxIntegrations = license.Plan.Limits.MaxIntegrations,
                CustomLimits = license.Plan.Limits.CustomLimits != null
                    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(license.Plan.Limits.CustomLimits)
                    : null,
                IsDefault = license.Plan.Limits.IsDefault,
                IsActive = license.Plan.Limits.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = license.Plan.Limits.CreatedBy,
                LastModifiedBy = license.Plan.Limits.LastModifiedBy
            } : null;

            var response = new LicenseValidationResponse
            {
                IsValid = true,
                Status = license.Status,
                ExpirationDate = license.ExpirationDate,
                DaysUntilExpiration = (int)(license.ExpirationDate - DateTime.UtcNow).TotalDays,
                PlanFeatures = enabledFeatures,
                PlanLimits = planLimits,
                Restrictions = new LicenseRestrictionsResponse
                {
                    HardwareId = license.HardwareId,
                    DomainRestrictions = license.DomainRestrictions?.Split(','),
                    IpRestrictions = license.IpRestrictions?.Split(',')
                },
                Metadata = license.Metadata != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) : null,
                ValidationId = Guid.NewGuid().ToString(),
                ValidatedAt = DateTime.UtcNow
            };

            return Result<LicenseValidationResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license {LicenseKey}", request.LicenseKey);
            return Result<LicenseValidationResponse>.Failure("An error occurred while validating the license", 500);
        }
    }

    // Analytics
    public async Task<Result<LicenseAnalyticsDto>> GetLicenseAnalyticsAsync()
    {
        try
        {
            var licenses = await _context.Licenses
                .Include(l => l.Plan)
                .ToListAsync();

            var totalLicenses = licenses.Count;
            var activeLicenses = licenses.Count(l => l.Status == LicenseStatusType.Active);
            var expiredLicenses = licenses.Count(l => l.Status == LicenseStatusType.Expired || l.ExpirationDate < DateTime.UtcNow);
            var suspendedLicenses = licenses.Count(l => l.Status == LicenseStatusType.Suspended);
            var revokedLicenses = licenses.Count(l => l.Status == LicenseStatusType.Revoked);
            var trialLicenses = licenses.Count(l => l.Type == LicenseType.Trial);

            var licensesExpiringIn30Days = licenses.Count(l => l.ExpirationDate <= DateTime.UtcNow.AddDays(30) && l.ExpirationDate >= DateTime.UtcNow);
            var licensesExpiringIn7Days = licenses.Count(l => l.ExpirationDate <= DateTime.UtcNow.AddDays(7) && l.ExpirationDate >= DateTime.UtcNow);

            var averageValidationCount = licenses.Any() ? licenses.Average(l => l.ValidationCount) : 0;

            var topValidatedLicenses = licenses
                .OrderByDescending(l => l.ValidationCount)
                .Take(5)
                .Select(l => new TopValidatedLicenseDto
                {
                    LicenseId = l.Id,
                    LicenseKey = l.LicenseKey,
                    TenantName = "Unknown", // Would need to join with tenants table
                    ValidationCount = l.ValidationCount
                })
                .ToArray();

            var licenseTypeDistribution = licenses
                .GroupBy(l => l.Type)
                .Select(g => new LicenseTypeDistributionDto
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / totalLicenses * 100
                })
                .ToArray();

            var monthlyIssuedLicenses = licenses
                .GroupBy(l => new { l.IssuedDate.Year, l.IssuedDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyIssuedLicenseDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .ToArray();

            var response = new LicenseAnalyticsDto
            {
                TotalLicenses = totalLicenses,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = expiredLicenses,
                SuspendedLicenses = suspendedLicenses,
                RevokedLicenses = revokedLicenses,
                TrialLicenses = trialLicenses,
                LicensesExpiringIn30Days = licensesExpiringIn30Days,
                LicensesExpiringIn7Days = licensesExpiringIn7Days,
                AverageValidationCount = (decimal)averageValidationCount,
                TopValidatedLicenses = topValidatedLicenses,
                LicenseTypeDistribution = licenseTypeDistribution,
                MonthlyIssuedLicenses = monthlyIssuedLicenses,
                RevenueByLicenseType = Array.Empty<RevenueByLicenseTypeDto>() // Would need pricing data
            };

            return Result<LicenseAnalyticsDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license analytics");
            return Result<LicenseAnalyticsDto>.Failure("An error occurred while getting license analytics", 500);
        }
    }

    public async Task<Result<LicenseUsageDto[]>> GetLicenseUsageAsync()
    {
        try
        {
            var licenses = await _context.Licenses
                .Include(l => l.Plan)
                .Include(l => l.Plan!.Limits)
                .ToListAsync();

            var usageData = licenses.Select(l => new LicenseUsageDto
            {
                LicenseId = l.Id,
                LicenseKey = l.LicenseKey,
                TenantName = "Unknown", // Would need to join with tenants table
                PlanName = l.Plan!.Name,
                Status = l.Status,
                IssuedDate = l.IssuedDate,
                ExpirationDate = l.ExpirationDate,
                DaysUntilExpiration = (int)(l.ExpirationDate - DateTime.UtcNow).TotalDays,
                ValidationCount = l.ValidationCount,
                MaxValidations = l.MaxValidations,
                LastValidated = l.LastValidated,
                CurrentUsage = new LicenseCurrentUsageDto
                {
                    Users = 0, // Would need to query actual usage
                    Branches = 0,
                    Rooms = 0,
                    Reservations = 0,
                    StorageGB = 0,
                    ApiCallsLast24h = 0
                },
                Limits = l.Plan.Limits != null ? new LimitsResponseDto
                {
                    Id = l.Plan.Limits.Id,
                    Name = l.Plan.Limits.Name,
                    Description = l.Plan.Limits.Description,
                    MaxUsers = l.Plan.Limits.MaxUsers,
                    MaxBranches = l.Plan.Limits.MaxBranches,
                    MaxRooms = l.Plan.Limits.MaxRooms,
                    MaxReservations = l.Plan.Limits.MaxReservations,
                    MaxStorageGB = l.Plan.Limits.MaxStorageGB,
                    ApiRateLimit = l.Plan.Limits.ApiRateLimit,
                    ConcurrentSessions = l.Plan.Limits.ConcurrentSessions,
                    CustomLimits = l.Plan.Limits.CustomLimits != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(l.Plan.Limits.CustomLimits) : null,
                    IsDefault = l.Plan.Limits.IsDefault,
                    IsActive = l.Plan.Limits.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = l.Plan.Limits.CreatedBy,
                    LastModifiedBy = l.Plan.Limits.LastModifiedBy
                } : null
            }).ToArray();

            return Result<LicenseUsageDto[]>.Success(usageData, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license usage");
            return Result<LicenseUsageDto[]>.Failure("An error occurred while getting license usage", 500);
        }
    }

    public async Task<Result<LicenseResponseDto[]>> GetExpiringLicensesAsync(int days)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(days);
            var expiringLicenses = await _context.Licenses
                .Include(l => l.Plan)
                .Where(l => l.ExpirationDate <= expiryDate && l.ExpirationDate >= DateTime.UtcNow)
                .OrderBy(l => l.ExpirationDate)
                .Select(l => new LicenseResponseDto
                {
                    Id = l.Id,
                    LicenseKey = l.LicenseKey,
                    PlanId = l.PlanId,
                    PlanName = l.Plan!.Name,
                    Type = l.Type,
                    Status = l.Status,
                    ExpirationDate = l.ExpirationDate,
                    MaxValidations = l.MaxValidations,
                    ValidationCount = l.ValidationCount,
                    HardwareId = l.HardwareId,
                    Created = l.IssuedDate,
                    CreatedBy = l.CreatedBy,
                    LastModified = l.LastValidated ?? l.IssuedDate,
                    LastModifiedBy = l.LastModifiedBy
                })
                .ToArrayAsync();

            return Result<LicenseResponseDto[]>.Success(expiringLicenses, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring licenses");
            return Result<LicenseResponseDto[]>.Failure("An error occurred while getting expiring licenses", 500);
        }
    }

    // Bulk Operations
    public async Task<Result<bool>> BulkRenewLicensesAsync(BulkLicenseRenewalRequest[] renewals)
    {
        try
        {
            var licenseIds = renewals.Select(r => r.LicenseId).ToList();
            var licenses = await _context.Licenses
                .Where(l => licenseIds.Contains(l.Id))
                .ToListAsync();

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var renewal in renewals)
            {
                var license = licenses.FirstOrDefault(l => l.Id == renewal.LicenseId);
                if (license == null)
                {
                    errors.Add($"License with ID {renewal.LicenseId} not found");
                    continue;
                }

                try
                {
                    license.ExpirationDate = renewal.NewExpirationDate;
                    license.Status = LicenseStatusType.Active;
                    license.LastValidated = DateTime.UtcNow;
                    license.LastModifiedBy = "SuperAdmin";

                    // Add renewal reason to metadata
                    var metadata = license.Metadata != null
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>();

                    metadata["RenewalReason"] = renewal.Reason;
                    metadata["RenewedAt"] = DateTime.UtcNow;
                    license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error renewing license {LicenseId} in bulk operation", renewal.LicenseId);
                    errors.Add($"Failed to renew license '{license.LicenseKey}': {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return Result<bool>.Failure($"Bulk renewal completed with {errors.Count} errors: {string.Join("; ", errors)}", 207);
            }

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk renew licenses");
            return Result<bool>.Failure("An error occurred during bulk renewal of licenses", 500);
        }
    }

    public async Task<Result<bool>> BulkSuspendLicensesAsync(BulkLicenseSuspensionRequest[] suspensions)
    {
        try
        {
            var licenseIds = suspensions.Select(s => s.LicenseId).ToList();
            var licenses = await _context.Licenses
                .Where(l => licenseIds.Contains(l.Id))
                .ToListAsync();

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var suspension in suspensions)
            {
                var license = licenses.FirstOrDefault(l => l.Id == suspension.LicenseId);
                if (license == null)
                {
                    errors.Add($"License with ID {suspension.LicenseId} not found");
                    continue;
                }

                try
                {
                    license.Status = LicenseStatusType.Suspended;
                    license.LastValidated = DateTime.UtcNow;
                    license.LastModifiedBy = "SuperAdmin";

                    // Add suspension reason to metadata
                    var metadata = license.Metadata != null
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(license.Metadata) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>();

                    metadata["SuspensionReason"] = suspension.Reason;
                    metadata["SuspendedAt"] = DateTime.UtcNow;
                    license.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error suspending license {LicenseId} in bulk operation", suspension.LicenseId);
                    errors.Add($"Failed to suspend license '{license.LicenseKey}': {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return Result<bool>.Failure($"Bulk suspension completed with {errors.Count} errors: {string.Join("; ", errors)}", 207);
            }

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk suspend licenses");
            return Result<bool>.Failure("An error occurred during bulk suspension of licenses", 500);
        }
    }
}
