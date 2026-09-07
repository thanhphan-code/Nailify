using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Api.Services;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/payments/payos")]
[AllowAnonymous]
public sealed class PayOsWebhooksController(NailifyDbContext db, PayOsService payOs, DepositLifecycleService lifecycle) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Receive([FromBody] JsonDocument document, CancellationToken ct)
    {
        var payload = document.RootElement.Deserialize<PayOsWebhook>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload?.Data is null || !document.RootElement.TryGetProperty("data", out var rawData)) return BadRequest();
        if (!payload.Success || payload.Code != "00" || payload.Data.Code != "00" || !payOs.IsValidWebhook(rawData, payload.Signature)) return BadRequest();

        var payment = await db.DepositPayments.Include(x => x.Appointment).ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.PayOsOrderCode == payload.Data.OrderCode, ct);
        if (payment is null || payment.Status == DepositPaymentStatus.Expired || payment.Status == DepositPaymentStatus.Approved || payment.Appointment.Status != AppointmentStatus.AwaitingDeposit) return Ok();
        if (payment.Amount != payload.Data.Amount || (!string.IsNullOrEmpty(payment.PayOsPaymentLinkId) && payment.PayOsPaymentLinkId != payload.Data.PaymentLinkId)) return BadRequest();

        lifecycle.Confirm(payment, payload.Data.Reference);
        await db.SaveChangesAsync(ct);
        await lifecycle.SendConfirmationEmailAsync(payment, ct);
        return Ok();
    }
}

public sealed record PayOsWebhook(string Code, bool Success, PayOsWebhookData Data, string Signature);
public sealed record PayOsWebhookData(long OrderCode, decimal Amount, string Code, string PaymentLinkId, string Reference);
