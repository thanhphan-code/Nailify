using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class StaffServiceConfiguration : IEntityTypeConfiguration<StaffService>
{
    public void Configure(EntityTypeBuilder<StaffService> builder)
    {
        builder.HasKey(x => new { x.StaffId, x.ServiceId });
        builder.HasOne(x => x.Staff).WithMany(x => x.StaffServices).HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Service).WithMany(x => x.StaffServices).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
