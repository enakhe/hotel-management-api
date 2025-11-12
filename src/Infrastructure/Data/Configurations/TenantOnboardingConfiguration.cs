using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class TenantOnboardingConfiguration : IEntityTypeConfiguration<TenantOnboarding>
{
    public void Configure(EntityTypeBuilder<TenantOnboarding> builder)
    {
        builder.Property(to => to.ContactPerson)
            .HasMaxLength(200);

        builder.Property(to => to.ContactEmail)
            .HasMaxLength(200);

        builder.Property(to => to.ContactPhone)
            .HasMaxLength(50);

        builder.Property(to => to.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(to => to.Tenant)
            .WithMany()
            .HasForeignKey(to => to.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(to => to.SelectedPlan)
            .WithMany()
            .HasForeignKey(to => to.SelectedPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(to => to.TenantId);
        builder.HasIndex(to => to.Status);
        builder.HasIndex(to => to.CurrentStep);
        builder.HasIndex(to => to.StartedAt);
    }
}


