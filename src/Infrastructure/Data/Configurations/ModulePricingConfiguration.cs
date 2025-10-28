using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class ModulePricingConfiguration : IEntityTypeConfiguration<ModulePricing>
{
    public void Configure(EntityTypeBuilder<ModulePricing> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Currency)
            .HasMaxLength(3);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        // Configure relationship
        builder.HasOne(p => p.Module)
            .WithOne(m => m.Pricing)
            .HasForeignKey<ModulePricing>(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ModuleId)
            .IsUnique();
    }
}
