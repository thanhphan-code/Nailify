using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class CustomerFavoriteDesignConfiguration : IEntityTypeConfiguration<CustomerFavoriteDesign>
{
    public void Configure(EntityTypeBuilder<CustomerFavoriteDesign> builder)
    {
        builder.HasKey(x => new { x.CustomerId, x.NailDesignId });
        builder.HasIndex(x => new { x.CustomerId, x.NailDesignId }).IsUnique();
        builder.HasOne(x => x.Customer).WithMany(x => x.FavoriteDesigns).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.NailDesign).WithMany(x => x.FavoritedByCustomers).HasForeignKey(x => x.NailDesignId).OnDelete(DeleteBehavior.Cascade);
    }
}
