using Microsoft.EntityFrameworkCore;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;
using Nailify.Application.Common.Interfaces;

namespace Nailify.Api.Services;

public sealed class DepositLifecycleService(NailifyDbContext db, PayOsService payOs, IEmailSender emailSender, ILogger<DepositLifecycleService> logger)
{
    public async Task<bool> ReconcileAsync(DepositPayment payment, CancellationToken ct)
    {
        if (payment.PayOsOrderCode is null || payment.Status == DepositPaymentStatus.Approved) return false;

        var remote = await payOs.GetPaymentStatus(payment.PayOsOrderCode.Value, ct);
        if (remote.OrderCode != payment.PayOsOrderCode || remote.Amount != payment.Amount) return false;

        if (remote.IsPaid && remote.AmountPaid >= payment.Amount)
        {
            Confirm(payment, payment.TransactionReference ?? $"PAYOS-{remote.OrderCode}");
            return true;
        }

        if (remote.IsCancelled && payment.Appointment.Status == AppointmentStatus.AwaitingDeposit)
        {
            Cancel(payment, DepositPaymentStatus.Rejected, "Khách hàng đã hủy thanh toán tiền cọc.");
            return true;
        }

        return false;
    }

    public void Confirm(DepositPayment payment, string transactionReference)
    {
        if (payment.Status == DepositPaymentStatus.Approved) return;
        payment.Status = DepositPaymentStatus.Approved;
        payment.TransactionReference = transactionReference;
        payment.VerifiedAt = DateTime.UtcNow;
        payment.Appointment.Status = AppointmentStatus.Confirmed;
        payment.Appointment.UpdatedAt = DateTime.UtcNow;
        db.Notifications.Add(new Notification
        {
            UserId = payment.Appointment.CustomerId,
            AppointmentId = payment.AppointmentId,
            Type = "DepositApproved",
            Title = "Thanh toán thành công",
            Message = "payOS đã xác nhận tiền cọc. Lịch hẹn của bạn đã được xác nhận tự động.",
            Link = "/appointments"
        });
    }

    public async Task SendConfirmationEmailAsync(DepositPayment payment, CancellationToken ct)
    {
        if (payment.Appointment.Customer is null || string.IsNullOrWhiteSpace(payment.Appointment.Customer.Email)) return;
        var date = payment.Appointment.AppointmentDate.ToString("dd/MM/yyyy");
        var time = payment.Appointment.StartTime.ToString("HH:mm");
        var amount = payment.Amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        try
        {
            await emailSender.SendAsync(payment.Appointment.Customer.Email, "Nailify xác nhận lịch hẹn",
                $"Nailify đã nhận {amount} ₫ tiền cọc. Lịch hẹn ngày {date} lúc {time} đã được xác nhận.",
                $"<h2>Lịch hẹn đã được xác nhận</h2><p>Nailify đã nhận <strong>{amount} ₫</strong> tiền cọc.</p><p>Thời gian: <strong>{date} lúc {time}</strong>.</p>", ct);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Không thể gửi email xác nhận lịch {AppointmentId}.", payment.AppointmentId);
        }
    }

    public void Cancel(DepositPayment payment, DepositPaymentStatus status, string reason)
    {
        payment.Status = status;
        payment.Appointment.Status = AppointmentStatus.Cancelled;
        payment.Appointment.CancellationReason = reason;
        payment.Appointment.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<bool> CancelRemoteAsync(DepositPayment payment, string reason, CancellationToken ct)
    {
        if (payment.PayOsOrderCode is null) return true;
        try
        {
            var remote = await payOs.CancelPaymentLink(payment.PayOsOrderCode.Value, reason, ct);
            return remote.IsCancelled;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(exception, "Không thể hủy link payOS cho đơn {OrderCode}.", payment.PayOsOrderCode);
            return false;
        }
    }
}
