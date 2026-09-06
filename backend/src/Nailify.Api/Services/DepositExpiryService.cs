using Microsoft.EntityFrameworkCore;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Services;

public sealed class DepositExpiryService(IServiceScopeFactory scopes, ILogger<DepositExpiryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<NailifyDbContext>();
                var lifecycle = scope.ServiceProvider.GetRequiredService<DepositLifecycleService>();
                var overdue = await db.DepositPayments.Include(x => x.Appointment).ThenInclude(x => x.Customer)
                    .Where(x => (x.Status == DepositPaymentStatus.AwaitingReceipt || x.Status == DepositPaymentStatus.ReceiptSubmitted)
                        && x.ExpiresAt <= DateTime.UtcNow && x.Appointment.Status == AppointmentStatus.AwaitingDeposit)
                    .ToListAsync(stoppingToken);

                foreach (var payment in overdue)
                {
                    if (payment.PayOsOrderCode.HasValue)
                    {
                        try
                        {
                            if (await lifecycle.ReconcileAsync(payment, stoppingToken))
                            {
                                await db.SaveChangesAsync(stoppingToken);
                                if (payment.Status == DepositPaymentStatus.Approved) await lifecycle.SendConfirmationEmailAsync(payment, stoppingToken);
                                continue;
                            }
                        }
                        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
                        {
                            logger.LogWarning(exception, "Chưa thể đối soát đơn payOS {OrderCode}.", payment.PayOsOrderCode);
                            continue;
                        }
                        if (!await lifecycle.CancelRemoteAsync(payment, "Hết thời hạn thanh toán 10 phút", stoppingToken)) continue;
                    }

                    lifecycle.Cancel(payment, DepositPaymentStatus.Expired, "Quá hạn thanh toán tiền cọc.");
                    db.Notifications.Add(new Notification { UserId = payment.Appointment.CustomerId, AppointmentId = payment.AppointmentId, Type = "DepositExpired", Title = "Lịch hẹn đã hết hạn", Message = "Hệ thống chưa nhận được tiền cọc trong 10 phút nên lịch đã tự hủy.", Link = "/appointments" });
                }
                if (overdue.Count > 0) await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Không thể xử lý các phiên đặt cọc hết hạn.");
            }
        }
    }
}
