using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(s => s.ImageUrl).HasMaxLength(2048);
        builder.Property(s => s.Price).HasColumnType("numeric(12,2)");
        builder.HasIndex(s => new { s.Category, s.Name }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_Services_Price_NonNegative", "\"Price\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Services_DurationMinutes_Positive", "\"DurationMinutes\" > 0"));
    }
}
