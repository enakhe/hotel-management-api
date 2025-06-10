using HotelManagement.Domain.Entities.Administrator;
using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    DbSet<Permission> Permissions { get; set; }

    DbSet<RolePermission> RolePermissions { get; set; }

    DbSet<AuditLog> AuditLogs { get; set; }

    DbSet<AuditLogDetail> AuditLogDetails { get; set; }

    DbSet<Branch> Branches { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
