using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(s => s.SubscriptionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SubscriptionNumber)
            .IsUnique();

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(s => s.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(s => s.CustomDiscount)
            .HasPrecision(5, 2);

        builder.Property(s => s.DiscountReason)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Invoices)
            .WithOne(i => i.Subscription)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.SubscriptionModules)
            .WithOne(sm => sm.Subscription)
            .HasForeignKey(sm => sm.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.PlanId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.NextBillingDate);
    }
}

internal class SubscriptionModuleConfiguration : IEntityTypeConfiguration<SubscriptionModule>
{
    public void Configure(EntityTypeBuilder<SubscriptionModule> builder)
    {
        builder.Property(sm => sm.Price)
            .HasPrecision(18, 2);

        // Composite key
        builder.HasKey(sm => new { sm.SubscriptionId, sm.ModuleId });

        // Relationships
        builder.HasOne(sm => sm.Module)
            .WithMany()
            .HasForeignKey(sm => sm.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(sm => sm.AddedAt);
        builder.HasIndex(sm => sm.RemovedAt);
    }
}


