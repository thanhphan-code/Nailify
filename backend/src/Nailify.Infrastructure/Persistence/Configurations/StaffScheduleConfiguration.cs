using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.HasIndex(x => new { x.StaffId, x.WorkDate, x.StartTime }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_StaffSchedules_EndTime_After_StartTime", "\"EndTime\" > \"StartTime\""));

        builder.HasOne(x => x.Staff)
            .WithMany(x => x.StaffSchedules)
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
