using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence.Configurations;

public class DepositPaymentConfiguration : IEntityTypeConfiguration<DepositPayment>
{
    public void Configure(EntityTypeBuilder<DepositPayment> builder)
    {
        builder.Property(x => x.Amount).HasColumnType("numeric(12,2)");
        builder.Property(x => x.ReceiptPath).HasMaxLength(300);
        builder.HasIndex(x => x.AppointmentId).IsUnique();
        builder.HasIndex(x => x.PayOsOrderCode).IsUnique();
        builder.HasIndex(x => new { x.Status, x.ExpiresAt });
        builder.HasOne(x => x.Appointment).WithOne(x => x.DepositPayment).HasForeignKey<DepositPayment>(x => x.AppointmentId).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(t => t.HasCheckConstraint("CK_DepositPayments_Amount_Positive", "\"Amount\" > 0"));
    }
}
