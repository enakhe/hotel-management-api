using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Description)
            .HasMaxLength(1000);

        builder.Property(rt => rt.ReportType)
            .IsRequired();

        builder.Property(rt => rt.TemplateConfig)
            .HasColumnType("nvarchar(max)");

        builder.Property(rt => rt.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(rt => rt.CreatedBy)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(rt => rt.ReportType);
        builder.HasIndex(rt => rt.IsPublic);
        builder.HasIndex(rt => rt.IsActive);
        builder.HasIndex(rt => rt.CreatedBy);
        builder.HasIndex(rt => new { rt.IsActive, rt.IsPublic });
    }
}

