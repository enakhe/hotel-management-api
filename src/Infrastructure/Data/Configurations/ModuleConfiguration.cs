using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Dependencies)
            .HasMaxLength(2000); // JSON array of module IDs

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(100);

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(m => m.Name)
            .IsUnique();

        builder.HasIndex(m => m.Category);

        builder.HasIndex(m => m.IsActive);

        builder.HasIndex(m => m.IsCore);

        // Configure relationships
        builder.HasMany(m => m.Features)
            .WithOne(f => f.Module)
            .HasForeignKey(f => f.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Pricing)
            .WithOne(p => p.Module)
            .HasForeignKey<ModulePricing>(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
