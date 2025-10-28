using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.RoomName)
            .HasMaxLength(100);

        builder.Property(r => r.RoomType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.Property(r => r.BaseRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Currency)
            .HasMaxLength(10);

        builder.Property(r => r.Amenities)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasDefaultValue(RoomStatus.Available);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.IsAvailable)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(r => new { r.TenantId, r.RoomNumber })
            .IsUnique();

        builder.HasIndex(r => r.TenantId);

        builder.HasIndex(r => r.BranchId);

        builder.HasIndex(r => r.RoomType);

        builder.HasIndex(r => r.Status);

        builder.HasIndex(r => r.IsAvailable);

        // Relationships
        builder.HasMany(r => r.Reservations)
            .WithOne(res => res.Room)
            .HasForeignKey(res => res.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
