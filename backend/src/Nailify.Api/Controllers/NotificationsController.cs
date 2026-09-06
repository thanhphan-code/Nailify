using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.Notifications;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(NailifyDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationListResponse>> Get(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await db.Notifications.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).Take(30)
            .Select(x => new NotificationDto(x.Id, x.Type, x.Title, x.Message, x.Link, x.ReadAt != null, x.CreatedAt)).ToListAsync(ct);

        if (User.IsInRole("Admin"))
        {
            var overdue = await db.Appointments.AsNoTracking()
                .Where(x => x.Status == AppointmentStatus.Pending && x.CreatedAt < DateTime.UtcNow.AddMinutes(-15))
                .OrderBy(x => x.CreatedAt).Take(10)
                .Select(x => new NotificationDto(x.Id, "BookingOverdue", "Lịch hẹn chờ xác nhận quá lâu", $"Lịch ngày {x.AppointmentDate:dd/MM/yyyy} lúc {x.StartTime:HH\\:mm} đã chờ quá 15 phút.", "/staff/appointments", false, x.CreatedAt.AddMinutes(15))).ToListAsync(ct);
            items = overdue.Concat(items).OrderByDescending(x => x.CreatedAt).Take(30).ToList();
        }
        return Ok(new NotificationListResponse(items.Count(x => !x.IsRead), items));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var item = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (item is null) return NotFound();
        item.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await db.Notifications.Where(x => x.UserId == userId && x.ReadAt == null).ExecuteUpdateAsync(x => x.SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
        return NoContent();
    }
}
