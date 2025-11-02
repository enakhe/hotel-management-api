using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
{
    public void Configure(EntityTypeBuilder<ReportSchedule> builder)
    {
        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rs => rs.Description)
            .HasMaxLength(1000);

        builder.Property(rs => rs.ReportType)
            .IsRequired();

        builder.Property(rs => rs.Format)
            .IsRequired();

        builder.Property(rs => rs.Frequency)
            .IsRequired();

        builder.Property(rs => rs.CronExpression)
            .HasMaxLength(100);

        builder.Property(rs => rs.Parameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(rs => rs.Filters)
            .HasColumnType("nvarchar(max)");

        builder.Property(rs => rs.EmailRecipients)
            .HasColumnType("nvarchar(max)");

        builder.Property(rs => rs.CreatedBy)
            .IsRequired();

        builder.Property(rs => rs.TimeZone)
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        // Indexes for performance
        builder.HasIndex(rs => rs.IsActive);
        builder.HasIndex(rs => rs.NextRunAt);
        builder.HasIndex(rs => rs.CreatedBy);
        builder.HasIndex(rs => rs.TenantId);
        builder.HasIndex(rs => new { rs.IsActive, rs.NextRunAt });
    }
}

