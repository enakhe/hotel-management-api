using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class UsageRecordConfiguration : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.Property(ur => ur.Quantity)
            .HasPrecision(18, 4);

        builder.Property(ur => ur.Cost)
            .HasPrecision(18, 2);

        builder.Property(ur => ur.Metadata)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(ur => ur.Tenant)
            .WithMany()
            .HasForeignKey(ur => ur.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Subscription)
            .WithMany()
            .HasForeignKey(ur => ur.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ur => ur.Invoice)
            .WithMany()
            .HasForeignKey(ur => ur.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(ur => ur.TenantId);
        builder.HasIndex(ur => ur.Metric);
        builder.HasIndex(ur => ur.RecordedAt);
        builder.HasIndex(ur => new { ur.TenantId, ur.BillingPeriodStart, ur.BillingPeriodEnd });
        builder.HasIndex(ur => ur.IsBilled);
    }
}

internal class UsageAggregationConfiguration : IEntityTypeConfiguration<UsageAggregation>
{
    public void Configure(EntityTypeBuilder<UsageAggregation> builder)
    {
        builder.Property(ua => ua.EmailOverageCost)
            .HasPrecision(18, 2);

        builder.Property(ua => ua.SmsOverageCost)
            .HasPrecision(18, 2);

        builder.Property(ua => ua.StorageUsedGB)
            .HasPrecision(18, 4);

        builder.Property(ua => ua.StorageIncludedGB)
            .HasPrecision(18, 4);

        builder.Property(ua => ua.StorageOverageGB)
            .HasPrecision(18, 4);

        builder.Property(ua => ua.StorageOverageCost)
            .HasPrecision(18, 2);

        builder.Property(ua => ua.ApiCallsOverageCost)
            .HasPrecision(18, 2);

        builder.Property(ua => ua.TotalOverageCost)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(ua => ua.Tenant)
            .WithMany()
            .HasForeignKey(ua => ua.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ua => ua.Subscription)
            .WithMany()
            .HasForeignKey(ua => ua.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ua => ua.Invoice)
            .WithMany()
            .HasForeignKey(ua => ua.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(ua => ua.TenantId);
        builder.HasIndex(ua => ua.SubscriptionId);
        builder.HasIndex(ua => new { ua.BillingPeriodStart, ua.BillingPeriodEnd });
        builder.HasIndex(ua => ua.IsFinalized);
        builder.HasIndex(ua => ua.IsBilled);
    }
}

internal class OveragePricingConfiguration : IEntityTypeConfiguration<OveragePricing>
{
    public void Configure(EntityTypeBuilder<OveragePricing> builder)
    {
        builder.Property(op => op.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(op => op.Description)
            .HasMaxLength(500);

        builder.Property(op => op.PricePerUnit)
            .HasPrecision(18, 6);

        builder.Property(op => op.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(op => op.CreatedBy)
            .HasMaxLength(200);

        builder.Property(op => op.UpdatedBy)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(op => op.Plan)
            .WithMany()
            .HasForeignKey(op => op.PlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(op => op.Metric);
        builder.HasIndex(op => op.PlanId);
        builder.HasIndex(op => op.IsActive);
    }
}

