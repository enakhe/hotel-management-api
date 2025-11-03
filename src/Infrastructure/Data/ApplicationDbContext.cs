using System.Linq.Expressions;
using System.Reflection;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IApplicationDbContext
{
    private readonly ITenantContext? _tenantContext;
    private readonly ISuperAdminContext? _superAdminContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext? tenantContext = null,
        ISuperAdminContext? superAdminContext = null) : base(options)
    {
        _tenantContext = tenantContext;
        _superAdminContext = superAdminContext;
    }
    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogDetail> AuditLogDetails { get; set; }
    public DbSet<Branch> Branches { get; set; }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    public DbSet<SuperAdminAuditLogEntity> SuperAdminAuditLogs { get; set; }

    // Plan Management
    public DbSet<Plan> Plans { get; set; }

    // Module entities
    public DbSet<Domain.Entities.Module> Modules { get; set; }
    public DbSet<ModuleFeature> ModuleFeatures { get; set; }
    public DbSet<ModulePricing> ModulePricing { get; set; }

    public DbSet<License> Licenses { get; set; }
    public DbSet<Limits> Limits { get; set; }
    public DbSet<PlanModule> PlanModules { get; set; }

    // Report Management
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportSubscription> ReportSubscriptions { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.ApplyConfiguration(new TenantConfiguration());
        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new ApplicationRoleConfiguration());
        builder.ApplyConfiguration(new PermissionConfiguration());
        builder.ApplyConfiguration(new RolePermissionConfiguration());
        builder.ApplyConfiguration(new AuditLogConfiguration());
        builder.ApplyConfiguration(new AuditLogDetailConfiguration());
        builder.ApplyConfiguration(new BranchConfiguration());

        builder.ApplyConfiguration(new RoomConfiguration());
        builder.ApplyConfiguration(new ReservationConfiguration());

        builder.ApplyConfiguration(new SuperAdminAuditLogConfiguration());

        // Plan Management Configurations
        builder.ApplyConfiguration(new PlanConfiguration());
        builder.ApplyConfiguration(new LicenseConfiguration());
        builder.ApplyConfiguration(new LimitsConfiguration());

        // Report Management Configurations
        builder.ApplyConfiguration(new ReportConfiguration());
        builder.ApplyConfiguration(new ReportScheduleConfiguration());
        builder.ApplyConfiguration(new ReportSubscriptionConfiguration());
        builder.ApplyConfiguration(new ReportTemplateConfiguration());
        builder.ApplyConfiguration(new ReportExecutionConfiguration());

        ConfigureTenantQueryFilters(builder);
    }

    private void ConfigureTenantQueryFilters(ModelBuilder builder)
    {
        // Apply query filters to all entities that implement ITenantEntity
        // This ensures automatic tenant isolation at the database query level
        // Filters are bypassed for SuperAdmin operations

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // Check if entity implements ITenantEntity interface
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Use reflection to call the generic SetQueryFilter method
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { builder });
            }
        }
    }

    /// <summary>
    /// Sets the tenant query filter for a specific entity type
    /// </summary>
    private void SetTenantQueryFilter<TEntity>(ModelBuilder builder) where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e =>
            // Skip filter if SuperAdmin context is active
            (_superAdminContext != null && _superAdminContext.IsSuperAdmin) ||
            // Skip filter if no tenant is resolved
            (_tenantContext == null || !_tenantContext.IsResolved) ||
            // Apply filter: only show entities belonging to current tenant
            e.TenantId == _tenantContext.TenantId!.Value
        );
    }
}
