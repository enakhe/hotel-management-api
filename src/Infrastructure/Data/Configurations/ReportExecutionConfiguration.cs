using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.HasKey(re => re.Id);

        builder.Property(re => re.Status)
            .IsRequired();

        builder.Property(re => re.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(re => re.StackTrace)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(re => re.ReportSchedule)
            .WithMany(rs => rs.Executions)
            .HasForeignKey(re => re.ReportScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(re => re.Report)
            .WithMany(r => r.Executions)
            .HasForeignKey(re => re.ReportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(re => re.ReportScheduleId);
        builder.HasIndex(re => re.ReportId);
        builder.HasIndex(re => re.Status);
        builder.HasIndex(re => re.StartedAt);
        builder.HasIndex(re => re.InitiatedBy);
        builder.HasIndex(re => re.IsScheduled);
        builder.HasIndex(re => new { re.Status, re.StartedAt });
        builder.HasIndex(re => new { re.ReportScheduleId, re.StartedAt });
    }
}

