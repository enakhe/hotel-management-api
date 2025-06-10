using HotelManagement.Domain.Entities.Configuration;
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
    }
}
