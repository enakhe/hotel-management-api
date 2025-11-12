using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class PaymentRetryConfiguration : IEntityTypeConfiguration<PaymentRetry>
{
    public void Configure(EntityTypeBuilder<PaymentRetry> builder)
    {
        builder.Property(pr => pr.FailureReason)
            .HasMaxLength(1000);

        builder.Property(pr => pr.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(pr => pr.Invoice)
            .WithMany()
            .HasForeignKey(pr => pr.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Tenant)
            .WithMany()
            .HasForeignKey(pr => pr.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pr => pr.InvoiceId);
        builder.HasIndex(pr => pr.TenantId);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(pr => pr.NextRetryAt);
        builder.HasIndex(pr => pr.AttemptedAt);
    }
}
