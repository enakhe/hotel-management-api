using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class ReportSubscriptionConfiguration : IEntityTypeConfiguration<ReportSubscription>
{
    public void Configure(EntityTypeBuilder<ReportSubscription> builder)
    {
        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.SuperAdminId)
            .IsRequired();

        builder.Property(rs => rs.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rs => rs.ReportScheduleId)
            .IsRequired();

        builder.Property(rs => rs.UnsubscribeToken)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(rs => rs.ReportSchedule)
            .WithMany(s => s.Subscriptions)
            .HasForeignKey(rs => rs.ReportScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(rs => rs.SuperAdminId);
        builder.HasIndex(rs => rs.Email);
        builder.HasIndex(rs => rs.ReportScheduleId);
        builder.HasIndex(rs => rs.UnsubscribeToken)
            .IsUnique();
        builder.HasIndex(rs => rs.IsActive);
        builder.HasIndex(rs => new { rs.SuperAdminId, rs.ReportScheduleId })
            .IsUnique();
    }
}

