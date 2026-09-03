using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(x => x.Comment).HasMaxLength(1_000);
        builder.HasIndex(x => x.AppointmentId).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating_Range", "\"Rating\" BETWEEN 1 AND 5"));

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
