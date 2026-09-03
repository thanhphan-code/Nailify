using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class ServiceNailDesignConfiguration : IEntityTypeConfiguration<ServiceNailDesign>
{
    public void Configure(EntityTypeBuilder<ServiceNailDesign> builder)
    {
        builder.ToTable("ServiceNailDesigns");
        builder.HasKey(x => new { x.ServiceId, x.NailDesignId });

        builder.HasOne(x => x.Service)
            .WithMany(x => x.ServiceNailDesigns)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NailDesign)
            .WithMany(x => x.ServiceNailDesigns)
            .HasForeignKey(x => x.NailDesignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
