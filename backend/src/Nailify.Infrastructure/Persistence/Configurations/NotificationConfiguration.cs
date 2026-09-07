using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Title).HasMaxLength(150);
        builder.Property(x => x.Message).HasMaxLength(500);
        builder.Property(x => x.Link).HasMaxLength(300);
        builder.HasIndex(x => new { x.UserId, x.ReadAt, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Appointment).WithMany(x => x.Notifications).HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
