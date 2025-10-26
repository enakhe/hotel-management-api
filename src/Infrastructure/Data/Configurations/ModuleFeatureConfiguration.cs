using HotelManagement.Domain.Entities.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class ModuleFeatureConfiguration : IEntityTypeConfiguration<ModuleFeature>
{
    public void Configure(EntityTypeBuilder<ModuleFeature> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.Configuration)
            .HasMaxLength(2000); // JSON configuration

        // Configure relationship
        builder.HasOne(f => f.Module)
            .WithMany(m => m.Features)
            .HasForeignKey(f => f.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.ModuleId);
    }
}
