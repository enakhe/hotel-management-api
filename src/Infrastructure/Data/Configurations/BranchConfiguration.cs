using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Address).HasMaxLength(200);
        builder.Property(b => b.ContactNumber).HasMaxLength(20);
        builder.Property(b => b.Email).HasMaxLength(100);
        builder.Property(b => b.TimeZone).HasMaxLength(50);
        builder.Property(b => b.CurrencyCode).HasMaxLength(10);

        // Indexes for query optimization
        builder.HasIndex(b => b.TenantId)
            .HasDatabaseName("IX_Branches_TenantId");

        builder.HasIndex(b => new { b.TenantId, b.IsActive })
            .HasDatabaseName("IX_Branches_TenantId_IsActive");

        builder.HasIndex(b => new { b.TenantId, b.Name })
            .HasDatabaseName("IX_Branches_TenantId_Name");

        builder.HasIndex(b => b.Email)
            .HasDatabaseName("IX_Branches_Email");
    }
}
