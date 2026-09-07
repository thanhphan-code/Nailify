using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.TotalPrice).HasColumnType("numeric(12,2)");
        builder.Property(a => a.DesignExtraPriceAtBooking).HasColumnType("numeric(12,2)");
        builder.Property(a => a.Note).HasMaxLength(500);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.HasIndex(a => new { a.StaffId, a.AppointmentDate, a.StartTime }); // ho tro check BR-003 nhanh hon
        builder.HasIndex(a => new { a.CustomerId, a.AppointmentDate });
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_EndTime_After_StartTime", "\"EndTime\" > \"StartTime\""));
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_TotalPrice_NonNegative", "\"TotalPrice\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_DesignExtraPrice_NonNegative", "\"DesignExtraPriceAtBooking\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_DesignAdditionalDuration_NonNegative", "\"DesignAdditionalDurationAtBooking\" >= 0"));

        builder.HasOne(x => x.NailDesign)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.NailDesignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
