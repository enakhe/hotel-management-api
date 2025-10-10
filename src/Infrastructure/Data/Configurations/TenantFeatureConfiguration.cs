using HotelManagement.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.HasKey(tf => tf.Id);

        builder.Property(tf => tf.FeatureName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tf => tf.Notes)
            .HasMaxLength(200);

        builder.Property(tf => tf.IsEnabled)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(tf => new { tf.TenantId, tf.FeatureName })
            .IsUnique();

        builder.HasIndex(tf => tf.FeatureName);

        // Relationships
        builder.HasOne(tf => tf.Tenant)
            .WithMany(t => t.Features)
            .HasForeignKey(tf => tf.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

