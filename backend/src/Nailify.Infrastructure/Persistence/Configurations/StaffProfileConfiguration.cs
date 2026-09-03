using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class StaffProfileConfiguration : IEntityTypeConfiguration<StaffProfile>
{
    public void Configure(EntityTypeBuilder<StaffProfile> builder)
    {
        builder.Property(x => x.Specialty).HasMaxLength(300);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2_048);
        builder.Property(x => x.Bio).HasMaxLength(2_000);
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(x => x.StaffProfile)
            .HasForeignKey<StaffProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
