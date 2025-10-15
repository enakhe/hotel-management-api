using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for tenant-related operations and validation
/// </summary>
public class TenantService(ApplicationDbContext context, ILogger<TenantService> logger) : ITenantService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<TenantService> _logger = logger;

    public async Task<bool> IsTenantValidAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            return tenant != null && tenant.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<Guid?> GetTenantIdByIdentifierAsync(string tenantIdentifier)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantIdentifier))
                return null;

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == tenantIdentifier && t.IsActive);

            return tenant?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant by identifier {TenantIdentifier}", tenantIdentifier);
            return null;
        }
    }

    public async Task<TenantInfo?> GetTenantInfoAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return null;

            return new TenantInfo
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.Created.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant info for {TenantId}", tenantId);
            return null;
        }
    }
}
