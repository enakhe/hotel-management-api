using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        // Decimal properties
        builder.Property(i => i.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.AmountPaid)
            .HasPrecision(18, 2);

        builder.Property(i => i.AmountDue)
            .HasPrecision(18, 2);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.PaymentInstructions)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Subscription)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.LineItems)
            .WithOne(li => li.Invoice)
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.SubscriptionId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => i.IssueDate);
    }
}

internal class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.Property(li => li.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Decimal properties
        builder.Property(li => li.Quantity)
            .HasPrecision(18, 4);

        builder.Property(li => li.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(li => li.Amount)
            .HasPrecision(18, 2);

        builder.Property(li => li.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(li => li.TaxAmount)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(li => li.Module)
            .WithMany()
            .HasForeignKey(li => li.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(li => li.Plan)
            .WithMany()
            .HasForeignKey(li => li.PlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(li => li.InvoiceId);
        builder.HasIndex(li => li.Type);
    }
}


