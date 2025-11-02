using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Type)
            .IsRequired();

        builder.Property(r => r.Category)
            .IsRequired();

        builder.Property(r => r.Format)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.GeneratedBy)
            .IsRequired();

        builder.Property(r => r.FilePath)
            .HasMaxLength(500);

        builder.Property(r => r.DownloadUrl)
            .HasMaxLength(1000);

        builder.Property(r => r.Parameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Filters)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(r => r.ReportSchedule)
            .WithMany(rs => rs.Reports)
            .HasForeignKey(r => r.ReportScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(r => r.GeneratedBy);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.GeneratedAt);
        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => r.ReportScheduleId);
        builder.HasIndex(r => new { r.Status, r.GeneratedAt });
        builder.HasIndex(r => new { r.GeneratedBy, r.GeneratedAt });
    }
}

