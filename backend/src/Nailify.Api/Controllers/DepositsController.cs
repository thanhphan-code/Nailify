using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Api.Services;
using Nailify.Contracts.Bookings;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/deposits")]
public sealed class DepositsController(NailifyDbContext db, PayOsService payOs, DepositLifecycleService lifecycle) : ControllerBase
{
    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("{appointmentId:guid}")]
    public async Task<ActionResult<DepositPaymentDto>> Get(Guid appointmentId, CancellationToken ct)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payment = await db.DepositPayments.Include(x => x.Appointment).ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.Appointment.CustomerId == customerId, ct);
        if (payment is null) return NotFound();

        if (payment.Status == DepositPaymentStatus.AwaitingReceipt && payment.ExpiresAt > DateTime.UtcNow && string.IsNullOrEmpty(payment.PayOsCheckoutUrl))
        {
            try
            {
                payment.PayOsOrderCode ??= await NextOrderCode(ct);
                await db.SaveChangesAsync(ct);
                var link = await payOs.CreatePaymentLink(payment.PayOsOrderCode.Value, appointmentId, payment.Amount, payment.ExpiresAt, ct);
                payment.PayOsPaymentLinkId = link.PaymentLinkId;
                payment.PayOsCheckoutUrl = link.CheckoutUrl;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException or TaskCanceledException)
            {
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Không thể kết nối cổng thanh toán payOS. Vui lòng thử lại.");
            }
        }

        if (payment.Status == DepositPaymentStatus.AwaitingReceipt && payment.PayOsOrderCode.HasValue && !string.IsNullOrEmpty(payment.PayOsCheckoutUrl))
        {
            try
            {
                if (await lifecycle.ReconcileAsync(payment, ct))
                {
                    await db.SaveChangesAsync(ct);
                    if (payment.Status == DepositPaymentStatus.Approved) await lifecycle.SendConfirmationEmailAsync(payment, ct);
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                // Webhook remains the primary source. A temporary status API failure must not hide the existing checkout link.
            }
        }

        return Ok(new DepositPaymentDto(payment.Id, payment.Amount, payment.Status.ToString(), payment.ExpiresAt, null, "", "", payment.PayOsCheckoutUrl));
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpPost("{appointmentId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid appointmentId, CancellationToken ct)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payment = await db.DepositPayments.Include(x => x.Appointment).ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.Appointment.CustomerId == customerId, ct);
        if (payment is null) return NotFound();
        if (payment.Status == DepositPaymentStatus.Approved || payment.Appointment.Status == AppointmentStatus.Confirmed)
            return Conflict(new { code = "PAYMENT_ALREADY_APPROVED", message = "Tiền cọc đã được xác nhận." });
        if (payment.Appointment.Status == AppointmentStatus.Cancelled) return NoContent();
        if (payment.Appointment.Status != AppointmentStatus.AwaitingDeposit)
            return Conflict(new { code = "INVALID_PAYMENT_STATE", message = "Không thể hủy thanh toán ở trạng thái hiện tại." });

        if (payment.PayOsOrderCode.HasValue)
        {
            try
            {
                if (await lifecycle.ReconcileAsync(payment, ct))
                {
                    await db.SaveChangesAsync(ct);
                    if (payment.Status == DepositPaymentStatus.Approved) await lifecycle.SendConfirmationEmailAsync(payment, ct);
                    return payment.Status == DepositPaymentStatus.Approved
                        ? Conflict(new { code = "PAYMENT_ALREADY_APPROVED", message = "Tiền cọc đã được xác nhận." })
                        : NoContent();
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Chưa thể kiểm tra trạng thái thanh toán với payOS. Vui lòng thử lại.");
            }

            if (!await lifecycle.CancelRemoteAsync(payment, "Khách hàng hủy thanh toán", ct))
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Chưa thể hủy link thanh toán payOS. Vui lòng thử lại.");
        }

        lifecycle.Cancel(payment, DepositPaymentStatus.Rejected, "Khách hàng đã hủy thanh toán tiền cọc.");
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<long> NextOrderCode(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = Random.Shared.NextInt64(10_000_000, 100_000_000);
            if (!await db.DepositPayments.AnyAsync(x => x.PayOsOrderCode == candidate, ct)) return candidate;
        }
        throw new InvalidOperationException("Could not allocate a unique payment order code.");
    }
}
