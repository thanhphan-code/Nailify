using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class AppointmentServiceConfiguration : IEntityTypeConfiguration<AppointmentService>
{
    public void Configure(EntityTypeBuilder<AppointmentService> builder)
    {
        builder.HasKey(x => new { x.AppointmentId, x.ServiceId });
        builder.Property(x => x.PriceAtBooking).HasColumnType("numeric(12,2)");
        builder.HasOne(x => x.Appointment).WithMany(x => x.AppointmentServices).HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Service).WithMany(x => x.AppointmentServices).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(t => t.HasCheckConstraint("CK_AppointmentServices_Price_NonNegative", "\"PriceAtBooking\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_AppointmentServices_Duration_Positive", "\"DurationAtBooking\" > 0"));
    }
}
