using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class NailDesignConfiguration : IEntityTypeConfiguration<NailDesign>
{
    public void Configure(EntityTypeBuilder<NailDesign> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2_048);
        builder.Property(x => x.ExtraPrice).HasColumnType("numeric(12,2)");
        builder.HasIndex(x => new { x.CategoryId, x.Name }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_NailDesigns_ExtraPrice_NonNegative", "\"ExtraPrice\" >= 0"));

        builder.HasOne(x => x.Category)
            .WithMany(x => x.NailDesigns)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
