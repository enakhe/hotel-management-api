using HotelManagement.Domain.Entities.Administrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;
internal class AuditLogDetailConfiguration : IEntityTypeConfiguration<AuditLogDetail>
{
    public void Configure(EntityTypeBuilder<AuditLogDetail> builder)
    {
        builder.Property(d => d.PropertyName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.OriginalValue).HasMaxLength(500);
        builder.Property(d => d.NewValue).HasMaxLength(500);

        builder.HasOne(d => d.AuditLog)
               .WithMany(l => l.PropertyChanges)
               .HasForeignKey(d => d.AuditLogId);
    }
}
