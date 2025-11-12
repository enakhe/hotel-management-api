using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.PaymentReference)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.PaymentReference)
            .IsUnique();

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.GatewayTransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.GatewayReference)
            .HasMaxLength(200);

        builder.Property(p => p.PaymentMethodDetails)
            .HasMaxLength(500);

        builder.Property(p => p.Last4Digits)
            .HasMaxLength(4);

        builder.Property(p => p.CardBrand)
            .HasMaxLength(50);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.ReconciledBy)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.GatewayTransactionId);
    }
}


