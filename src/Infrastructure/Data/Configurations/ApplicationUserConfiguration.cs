using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.HasOne(u => u.Branch)
               .WithMany(b => b.Users)
               .HasForeignKey(u => u.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes for query optimization
        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_AspNetUsers_TenantId");

        builder.HasIndex(u => new { u.TenantId, u.IsActive })
            .HasDatabaseName("IX_AspNetUsers_TenantId_IsActive");

        builder.HasIndex(u => u.BranchId)
            .HasDatabaseName("IX_AspNetUsers_BranchId");

        builder.HasIndex(u => new { u.TenantId, u.BranchId, u.IsActive })
            .HasDatabaseName("IX_AspNetUsers_TenantId_BranchId_IsActive");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_AspNetUsers_Email");

        builder.HasIndex(u => u.FullName)
            .HasDatabaseName("IX_AspNetUsers_FullName");
    }
}
