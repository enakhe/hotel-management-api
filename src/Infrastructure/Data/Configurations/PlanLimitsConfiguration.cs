using HotelManagement.Domain.Entities.SuperAdmin;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

public class PlanLimitsConfiguration : IEntityTypeConfiguration<PlanLimits>
{
    public void Configure(EntityTypeBuilder<PlanLimits> builder)
    {
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.MaxUsers)
            .IsRequired();

        builder.Property(pl => pl.MaxBranches)
            .IsRequired();

        builder.Property(pl => pl.MaxRooms)
            .IsRequired();

        builder.Property(pl => pl.MaxReservations)
            .IsRequired();

        builder.Property(pl => pl.MaxStorageGB)
            .IsRequired();

        builder.Property(pl => pl.ApiRateLimit)
            .IsRequired();

        builder.Property(pl => pl.SupportLevel)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(pl => pl.SLA)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(pl => pl.PlanId)
            .IsRequired();

        // Relationships
        builder.HasOne(pl => pl.Plan)
            .WithOne(p => p.Limits)
            .HasForeignKey<PlanLimits>(pl => pl.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pl => pl.PlanId)
            .IsUnique();
    }
}
