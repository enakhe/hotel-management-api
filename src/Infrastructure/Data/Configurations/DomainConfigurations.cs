using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Limits entity
/// </summary>
public class LimitsConfiguration : IEntityTypeConfiguration<Limits>
{
    public void Configure(EntityTypeBuilder<Limits> builder)
    {
        builder.ToTable("Limits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CustomLimits)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsDefault);
        builder.HasIndex(x => x.IsActive);

        // Relationships
        builder.HasMany(x => x.Plans)
            .WithOne(x => x.Limits)
            .HasForeignKey(x => x.LimitsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// EF Core configuration for Plan entity
/// </summary>
public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.BillingCycle)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsPopular)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsPopular);
        builder.HasIndex(x => x.LimitsId);

        // Relationships
        builder.HasOne(x => x.Limits)
            .WithMany(x => x.Plans)
            .HasForeignKey(x => x.LimitsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tenants)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Licenses)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlanModules)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for Tenant entity
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        // Ignore inherited TenantId property - a Tenant doesn't belong to another tenant
        builder.Ignore(x => x.TenantId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Identifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.Address)
            .HasMaxLength(200);

        builder.Property(x => x.ContactNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.TimeZone)
            .HasMaxLength(50)
            .HasDefaultValue("WAT");

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("NGN");

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.Industry)
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .HasMaxLength(50);

        builder.Property(x => x.Region)
            .HasMaxLength(50);

        // Foreign Keys
        builder.Property(x => x.PlanId)
            .IsRequired();

        builder.Property(x => x.LicenseId)
            .IsRequired();

        // Audit properties (inherited from BaseTenantAuditableEntity)
        builder.Property(x => x.Created)
            .IsRequired();

        builder.Property(x => x.LastModified)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.Identifier).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Country);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.PlanId);
        builder.HasIndex(x => x.LicenseId);

        // Relationships
        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Tenants)
            .HasForeignKey(x => x.PlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.License)
            .WithMany()
            .HasForeignKey(x => x.LicenseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Branches)
            .WithOne()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for License entity
/// </summary>
public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicenseKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.IssuedDate)
            .IsRequired();

        builder.Property(x => x.ExpirationDate)
            .IsRequired();

        builder.Property(x => x.LastValidated)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.DomainRestrictions)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IpRestrictions)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(x => x.LicenseKey).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.PlanId);
        builder.HasIndex(x => x.ExpirationDate);

        // Relationships
        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Validations)
            .WithOne(x => x.License)
            .HasForeignKey(x => x.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for PlanModule entity
/// </summary>
public class PlanModuleConfiguration : IEntityTypeConfiguration<PlanModule>
{
    public void Configure(EntityTypeBuilder<PlanModule> builder)
    {
        builder.ToTable("PlanModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.Property(x => x.AddedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Plan)
            .WithMany(x => x.PlanModules)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.PlanModules)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index to prevent duplicate plan-module combinations
        builder.HasIndex(x => new { x.PlanId, x.ModuleId })
            .IsUnique();

        // Indexes
        builder.HasIndex(x => x.PlanId);
        builder.HasIndex(x => x.ModuleId);
        builder.HasIndex(x => x.IsRequired);
        builder.HasIndex(x => x.DisplayOrder);
    }
}
