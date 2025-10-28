using System.Reflection;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options), IApplicationDbContext
{
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

        ConfigureTenantQueryFilters(builder);
    }

    private void ConfigureTenantQueryFilters(ModelBuilder builder)
    {
        // Note: We'll configure these filters dynamically based on the current tenant context
        // This is done in the TenantQueryFilterService to avoid circular dependencies
    }
}
