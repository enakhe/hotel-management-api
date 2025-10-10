using HotelManagement.Domain.Entities.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class SuperAdminAuditLogConfiguration : IEntityTypeConfiguration<SuperAdminAuditLogEntity>
{
    public void Configure(EntityTypeBuilder<SuperAdminAuditLogEntity> builder)
    {
        builder.HasKey(log => log.Id);

        builder.Property(log => log.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(log => log.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(log => log.TargetType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(log => log.TargetId)
            .HasMaxLength(50);

        builder.Property(log => log.Details)
            .HasMaxLength(1000);

        builder.Property(log => log.Changes)
            .HasColumnType("nvarchar(max)");

        builder.Property(log => log.IpAddress)
            .HasMaxLength(45);

        builder.Property(log => log.UserAgent)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(log => log.SuperAdminId);
        builder.HasIndex(log => log.TenantId);
        builder.HasIndex(log => log.Timestamp);
        builder.HasIndex(log => log.Action);
        builder.HasIndex(log => log.TargetType);
        builder.HasIndex(log => new { log.SuperAdminId, log.Timestamp });
        builder.HasIndex(log => new { log.TenantId, log.Timestamp });
    }
}
