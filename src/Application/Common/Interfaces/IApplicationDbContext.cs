using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Permission> Permissions { get; set; }

    DbSet<RolePermission> RolePermissions { get; set; }

    DbSet<AuditLog> AuditLogs { get; set; }

    DbSet<AuditLogDetail> AuditLogDetails { get; set; }

    DbSet<Branch> Branches { get; set; }

    // Tenant entities
    DbSet<Tenant> Tenants { get; set; }

    DbSet<ApplicationUser> Users { get; set; }

    // Plan and License entities
    DbSet<Plan> Plans { get; set; }

    DbSet<License> Licenses { get; set; }

    DbSet<Domain.Entities.Module> Modules { get; set; }

    // Business entities
    DbSet<Room> Rooms { get; set; }

    DbSet<Reservation> Reservations { get; set; }

    // Report Management entities
    DbSet<Report> Reports { get; set; }

    DbSet<ReportSchedule> ReportSchedules { get; set; }

    DbSet<ReportSubscription> ReportSubscriptions { get; set; }

    DbSet<ReportTemplate> ReportTemplates { get; set; }

    DbSet<ReportExecution> ReportExecutions { get; set; }

    // Audit entities
    DbSet<SuperAdminAuditLogEntity> SuperAdminAuditLogs { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
