using HotelManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data.Services;

/// <summary>
/// Factory for creating tenant-aware DbContext instances
/// </summary>
public class TenantAwareDbContextFactory
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ITenantContext _tenantContext;
    private readonly TenantQueryFilterService _queryFilterService;
    private readonly ILogger<TenantAwareDbContextFactory> _logger;

    public TenantAwareDbContextFactory(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        TenantQueryFilterService queryFilterService,
        ILogger<TenantAwareDbContextFactory> logger)
    {
        _options = options;
        _tenantContext = tenantContext;
        _queryFilterService = queryFilterService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new DbContext instance with tenant filters applied
    /// </summary>
    public ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(_options);

        // Apply tenant filters if tenant context is resolved
        if (_tenantContext.IsResolved)
        {
            _queryFilterService.ApplyTenantFilters(context);
        }

        return context;
    }
}

