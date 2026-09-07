using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.Bookings;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Authorize(Policy = "StaffOrAdmin")]
[Route("api/staff/appointments")]
public class StaffAppointmentsController(NailifyDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerAppointmentDto>>> GetAssigned(CancellationToken ct)
    {
        var staffId = CurrentUserId();
        var appointments = await db.Appointments.AsNoTracking()
            .Include(x => x.AppointmentServices).ThenInclude(x => x.Service)
            .Include(x => x.NailDesign)
            .Include(x => x.Staff)
            .Include(x => x.DepositPayment)
            .Where(x => User.IsInRole("Admin") || x.StaffId == staffId)
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.StartTime)
            .ToListAsync(ct);
        return Ok(appointments.Select(x =>
        {
            var service = x.AppointmentServices.FirstOrDefault();
            return new CustomerAppointmentDto(x.Id, $"APT-{x.AppointmentDate:yyyyMMdd}-{x.Id.ToString()[..6].ToUpperInvariant()}", x.AppointmentDate,
                x.StartTime, x.EndTime, x.Status.ToString(), service?.ServiceId ?? Guid.Empty, service?.Service.Name ?? "Dịch vụ làm nail",
                x.NailDesignId, x.NailDesign?.Name, x.StaffId, x.Staff?.FullName, service?.PriceAtBooking ?? 0,
                x.DesignExtraPriceAtBooking, x.TotalPrice, x.Note, x.CancellationReason, x.CreatedAt, null,
                x.DepositPayment is null ? null : new DepositPaymentDto(x.DepositPayment.Id, x.DepositPayment.Amount, x.DepositPayment.Status.ToString(), x.DepositPayment.ExpiresAt, x.DepositPayment.ReceiptPath, "", ""));
        }).ToList());
    }

    [HttpPatch("{id:guid}/confirm")]
    public Task<ActionResult<UpdateAppointmentStatusDto>> Confirm(Guid id, CancellationToken ct) =>
        Transition(id, AppointmentStatus.Pending, AppointmentStatus.Confirmed, ct);

    [HttpPatch("{id:guid}/start")]
    public Task<ActionResult<UpdateAppointmentStatusDto>> Start(Guid id, CancellationToken ct) =>
        Transition(id, AppointmentStatus.Confirmed, AppointmentStatus.InProgress, ct);

    [HttpPatch("{id:guid}/complete")]
    public Task<ActionResult<UpdateAppointmentStatusDto>> Complete(Guid id, CancellationToken ct) =>
        Transition(id, AppointmentStatus.InProgress, AppointmentStatus.Completed, ct);

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<UpdateAppointmentStatusDto>> Cancel(Guid id, CancelAppointmentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500)
            return BadRequest(new { code = "CANCELLATION_REASON_REQUIRED" });
        var appointment = await AssignedAppointment(id, ct);
        if (appointment is null) return NotFound();
        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            return Conflict(new { code = "APPOINTMENT_CANNOT_BE_CANCELLED" });
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;
        db.Notifications.Add(CustomerNotification(appointment, "BookingCancelled", "Lịch hẹn đã bị hủy", $"Tiệm đã hủy lịch ngày {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:HH\\:mm}. Lý do: {request.Reason.Trim()}"));
        await db.SaveChangesAsync(ct);
        return Ok(ToStatusDto(appointment));
    }

    private async Task<ActionResult<UpdateAppointmentStatusDto>> Transition(Guid id, AppointmentStatus required, AppointmentStatus next, CancellationToken ct)
    {
        var appointment = await AssignedAppointment(id, ct);
        if (appointment is null) return NotFound();
        if (appointment.Status != required) return Conflict(new { code = "INVALID_STATUS_TRANSITION", currentStatus = appointment.Status.ToString(), requiredStatus = required.ToString() });
        appointment.Status = next;
        appointment.UpdatedAt = DateTime.UtcNow;
        var (title, message) = next switch
        {
            AppointmentStatus.Confirmed => ("Lịch hẹn đã được xác nhận", $"Hẹn ngày {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:HH\\:mm} đã được tiệm xác nhận."),
            AppointmentStatus.InProgress => ("Dịch vụ đã bắt đầu", "Nhân viên đã bắt đầu thực hiện dịch vụ của bạn."),
            AppointmentStatus.Completed => ("Dịch vụ đã hoàn thành", "Cảm ơn bạn đã đến Nailify. Bạn có thể đánh giá dịch vụ trong mục Cuộc hẹn của tôi."),
            _ => ("Lịch hẹn được cập nhật", $"Trạng thái mới: {next}.")
        };
        db.Notifications.Add(CustomerNotification(appointment, $"Booking{next}", title, message));
        await db.SaveChangesAsync(ct);
        return Ok(ToStatusDto(appointment));
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Task<Nailify.Domain.Entities.Appointment?> AssignedAppointment(Guid id, CancellationToken ct) =>
        db.Appointments.FirstOrDefaultAsync(x => x.Id == id && (User.IsInRole("Admin") || x.StaffId == CurrentUserId()), ct);
    private static Nailify.Domain.Entities.Notification CustomerNotification(Nailify.Domain.Entities.Appointment appointment, string type, string title, string message) =>
        new() { UserId = appointment.CustomerId, AppointmentId = appointment.Id, Type = type, Title = title, Message = message, Link = "/appointments" };
    private static UpdateAppointmentStatusDto ToStatusDto(Nailify.Domain.Entities.Appointment appointment) =>
        new(appointment.Id, appointment.Status.ToString(), appointment.UpdatedAt);
}
