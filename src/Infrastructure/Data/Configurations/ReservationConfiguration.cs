using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReservationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.GuestName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.GuestEmail)
            .HasMaxLength(100);

        builder.Property(r => r.GuestPhone)
            .HasMaxLength(20);

        builder.Property(r => r.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.PaidAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Currency)
            .HasMaxLength(10);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasDefaultValue(ReservationStatus.Confirmed);

        builder.Property(r => r.PaymentStatus)
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(r => r.SpecialRequests)
            .HasMaxLength(500);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.Property(r => r.PaymentMethod)
            .HasMaxLength(100);

        builder.Property(r => r.CancellationReason)
            .HasMaxLength(500);

        builder.Property(r => r.CancellationFee)
            .HasColumnType("decimal(18,2)");

        // Indexes
        builder.HasIndex(r => new { r.TenantId, r.ReservationNumber })
            .IsUnique();

        builder.HasIndex(r => r.TenantId);

        builder.HasIndex(r => r.BranchId);

        builder.HasIndex(r => r.RoomId);

        builder.HasIndex(r => r.GuestEmail);

        builder.HasIndex(r => r.CheckInDate);

        builder.HasIndex(r => r.CheckOutDate);

        builder.HasIndex(r => r.Status);

        builder.HasIndex(r => r.PaymentStatus);

        // Relationships
        builder.HasOne(r => r.Room)
            .WithMany(room => room.Reservations)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
