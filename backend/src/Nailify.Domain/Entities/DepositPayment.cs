using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

public class DepositPayment : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public decimal Amount { get; set; }
    public DepositPaymentStatus Status { get; set; } = DepositPaymentStatus.AwaitingReceipt;
    public DateTime ExpiresAt { get; set; }
    public long? PayOsOrderCode { get; set; }
    public string? PayOsPaymentLinkId { get; set; }
    public string? PayOsCheckoutUrl { get; set; }
    public string? TransactionReference { get; set; }
    public string? ReceiptPath { get; set; }
    public DateTime? ReceiptSubmittedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
