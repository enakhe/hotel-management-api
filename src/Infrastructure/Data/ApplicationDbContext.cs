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

    // Billing & Subscription Management
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionModule> SubscriptionModules { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentRetry> PaymentRetries { get; set; }
    public DbSet<TenantOnboarding> TenantOnboardings { get; set; }

    // Usage Tracking
    public DbSet<UsageRecord> UsageRecords { get; set; }
    public DbSet<UsageAggregation> UsageAggregations { get; set; }
    public DbSet<OveragePricing> OveragePricings { get; set; }

    // Notification Management
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

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

        // Billing & Subscription Configurations
        builder.ApplyConfiguration(new SubscriptionConfiguration());
        builder.ApplyConfiguration(new SubscriptionModuleConfiguration());
        builder.ApplyConfiguration(new InvoiceConfiguration());
        builder.ApplyConfiguration(new InvoiceLineItemConfiguration());
        builder.ApplyConfiguration(new PaymentConfiguration());
        builder.ApplyConfiguration(new PaymentRetryConfiguration());
        builder.ApplyConfiguration(new TenantOnboardingConfiguration());

        // Usage Tracking Configurations
        builder.ApplyConfiguration(new UsageRecordConfiguration());
        builder.ApplyConfiguration(new UsageAggregationConfiguration());
        builder.ApplyConfiguration(new OveragePricingConfiguration());

        // Notification Configurations
        builder.ApplyConfiguration(new NotificationEntityConfiguration());
        builder.ApplyConfiguration(new NotificationDeliveryConfiguration());
        builder.ApplyConfiguration(new NotificationTemplateConfiguration());
        builder.ApplyConfiguration(new UserNotificationPreferenceConfiguration());

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
            // Exclude Tenant entity itself - it's the registry, not a tenant-scoped entity
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType) &&
                entityType.ClrType != typeof(Tenant))
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
    /// SuperAdmin bypass: When IsSuperAdmin == true, ALL entities across ALL tenants are visible
    /// Tenant isolation: When a tenant is resolved, only that tenant's entities are visible
    /// No tenant: When no tenant is resolved, no filtering is applied (e.g., anonymous requests)
    /// </summary>
    private void SetTenantQueryFilter<TEntity>(ModelBuilder builder) where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e =>
            // Skip filter if SuperAdmin context is active - SuperAdmin sees ALL tenants
            (_superAdminContext != null && _superAdminContext.IsSuperAdmin) ||
            // Skip filter if no tenant is resolved
            (_tenantContext == null || !_tenantContext.IsResolved) ||
            // Apply filter: only show entities belonging to current tenant
            e.TenantId == _tenantContext.TenantId!.Value
        );
    }
}
