using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Data.Configurations;

internal class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Message)
            .IsRequired();

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.Deliveries)
            .WithOne(d => d.Notification)
            .HasForeignKey(d => d.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}

internal class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.Property(nd => nd.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(nd => nd.ExternalId)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(nd => nd.NotificationId);
        builder.HasIndex(nd => nd.Channel);
        builder.HasIndex(nd => nd.Status);
        builder.HasIndex(nd => new { nd.NotificationId, nd.Channel });
    }
}

internal class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(nt => nt.TemplateKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(nt => nt.TemplateKey);
        builder.HasIndex(nt => new { nt.TenantId, nt.TemplateKey })
            .IsUnique();

        builder.Property(nt => nt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(nt => nt.Description)
            .HasMaxLength(500);

        builder.Property(nt => nt.EmailSubject)
            .HasMaxLength(200);

        builder.Property(nt => nt.SmsBody)
            .HasMaxLength(500);

        builder.Property(nt => nt.PushTitle)
            .HasMaxLength(500);

        builder.Property(nt => nt.PushBody)
            .HasMaxLength(500);

        builder.Property(nt => nt.Language)
            .HasMaxLength(10);

        builder.Property(nt => nt.CreatedBy)
            .HasMaxLength(200);

        builder.Property(nt => nt.UpdatedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(nt => nt.Category);
        builder.HasIndex(nt => nt.IsActive);
        builder.HasIndex(nt => nt.IsSystem);
    }
}

internal class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.Property(unp => unp.PreferredLanguage)
            .HasMaxLength(10);

        // Composite unique index
        builder.HasIndex(unp => new { unp.UserId, unp.Category })
            .IsUnique();

        // Relationships
        builder.HasOne(unp => unp.User)
            .WithMany()
            .HasForeignKey(unp => unp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(unp => unp.UserId);
    }
}

