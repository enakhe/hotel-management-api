using HotelManagement.Domain.Entities.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pf => pf.Description)
            .HasMaxLength(500);

        builder.Property(pf => pf.Included)
            .IsRequired();

        builder.Property(pf => pf.Limit);

        builder.Property(pf => pf.Unit)
            .HasMaxLength(50);

        builder.Property(pf => pf.PlanId)
            .IsRequired();

        // Relationships
        builder.HasOne(pf => pf.Plan)
            .WithMany(p => p.Features)
            .HasForeignKey(pf => pf.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pf => pf.PlanId);
    }
}
