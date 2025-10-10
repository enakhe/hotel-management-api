using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        // Basic Information
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(200);

        builder.Property(t => t.Address)
            .HasMaxLength(200);

        builder.Property(t => t.ContactNumber)
            .HasMaxLength(20);

        builder.Property(t => t.Email)
            .HasMaxLength(100);

        // Tenant Registry Configuration
        builder.Property(t => t.TimeZone)
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        builder.Property(t => t.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(t => t.LanguageCode)
            .HasMaxLength(10)
            .HasDefaultValue("en");

        // License & Subscription
        builder.Property(t => t.SubscriptionPlan)
            .HasMaxLength(50)
            .HasDefaultValue("Basic");

        builder.Property(t => t.LicenseStatus)
            .HasConversion<string>()
            .HasDefaultValue(LicenseStatus.Trial);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        // Resource Limits
        builder.Property(t => t.MaxUsers)
            .HasDefaultValue(10);

        builder.Property(t => t.MaxBranches)
            .HasDefaultValue(1);

        builder.Property(t => t.MaxRooms)
            .HasDefaultValue(100);

        builder.Property(t => t.MaxReservations)
            .HasDefaultValue(1000);

        // Database Configuration
        builder.Property(t => t.DatabaseConnectionString)
            .HasMaxLength(200);

        builder.Property(t => t.DatabaseProvider)
            .HasMaxLength(50)
            .HasDefaultValue("SqlServer");

        builder.Property(t => t.UseSharedDatabase)
            .HasDefaultValue(true);

        // Feature Flags (JSON)
        builder.Property(t => t.FeatureFlags)
            .HasColumnType("nvarchar(max)");

        // Tenant Metadata
        builder.Property(t => t.Industry)
            .HasMaxLength(100);

        builder.Property(t => t.Country)
            .HasMaxLength(50);

        builder.Property(t => t.Region)
            .HasMaxLength(50);

        // Billing Information
        builder.Property(t => t.BillingContactName)
            .HasMaxLength(100);

        builder.Property(t => t.BillingEmail)
            .HasMaxLength(100);

        builder.Property(t => t.BillingAddress)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(t => t.Identifier)
            .IsUnique();

        builder.HasIndex(t => t.Name);

        builder.HasIndex(t => t.LicenseStatus);

        builder.HasIndex(t => t.Country);

        builder.HasIndex(t => t.IsActive);

        // Relationships
        builder.HasMany(t => t.Branches)
            .WithOne(b => b.Tenant)
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Features)
            .WithOne(f => f.Tenant)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

