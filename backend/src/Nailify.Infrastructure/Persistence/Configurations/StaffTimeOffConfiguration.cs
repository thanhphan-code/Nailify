using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class StaffTimeOffConfiguration : IEntityTypeConfiguration<StaffTimeOff>
{
    public void Configure(EntityTypeBuilder<StaffTimeOff> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => new { x.StaffId, x.Date, x.StartTime });
        builder.ToTable(t => t.HasCheckConstraint("CK_StaffTimeOffs_EndTime_After_StartTime", "\"EndTime\" > \"StartTime\""));
        builder.HasOne(x => x.Staff).WithMany(x => x.StaffTimeOffs).HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
    }
}
