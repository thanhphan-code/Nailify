using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.Bookings;
using Nailify.Domain.Booking;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;
using Npgsql;

namespace Nailify.Api.Controllers;

[ApiController]
public class BookingsController(NailifyDbContext db, IConfiguration configuration) : ControllerBase
{
    private const int SlotIntervalMinutes = 30;
    private const int MaxAdvanceDays = 30;

    [AllowAnonymous]
    [HttpGet("api/services/{serviceId:guid}/staff")]
    public async Task<ActionResult<IReadOnlyList<StaffBookingDto>>> GetStaff(Guid serviceId, CancellationToken ct) => Ok(await db.StaffServices.AsNoTracking()
        .Where(x => x.ServiceId == serviceId && x.Staff.Role == UserRole.Staff && x.Staff.Status == UserStatus.Active)
        .OrderBy(x => x.Staff.FullName).Select(x => new StaffBookingDto(x.StaffId, x.Staff.FullName, x.Staff.StaffProfile == null ? null : x.Staff.StaffProfile.Specialty)).ToListAsync(ct));

    [AllowAnonymous]
    [HttpGet("api/bookings/available-slots")]
    public async Task<ActionResult<AvailableSlotsResponse>> AvailableSlots([FromQuery] Guid serviceId, [FromQuery] Guid? nailDesignId, [FromQuery] Guid? staffId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var service = await db.Services.AsNoTracking().FirstOrDefaultAsync(x => x.Id == serviceId && x.IsActive && x.IsBookable, ct);
        if (service is null) return NotFound(new { code = "SERVICE_NOT_AVAILABLE" });
        if (!ValidDate(date)) return BadRequest(new { code = "INVALID_BOOKING_DATE" });

        var designDuration = 0;
        if (nailDesignId.HasValue)
        {
            var design = await db.NailDesigns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == nailDesignId && x.Status == UserStatus.Active && x.IsBookable, ct);
            if (design is null || !await db.ServiceNailDesigns.AnyAsync(x => x.ServiceId == serviceId && x.NailDesignId == nailDesignId, ct))
                return BadRequest(new { code = "INCOMPATIBLE_SERVICE_DESIGN" });
            designDuration = design.AdditionalDurationMinutes;
        }

        var duration = BookingRules.TotalDurationMinutes(service.DurationMinutes, designDuration);
        var customerId = TryCurrentCustomerId();
        var staff = await EligibleStaff(serviceId, staffId, ct);
        var staffIds = staff.Select(x => x.Id).ToList();
        var schedules = await db.StaffSchedules.AsNoTracking().Where(x => staffIds.Contains(x.StaffId) && x.WorkDate == date).ToListAsync(ct);
        var timeOffs = await db.StaffTimeOffs.AsNoTracking().Where(x => staffIds.Contains(x.StaffId) && x.Date == date).ToListAsync(ct);
        var staffAppointments = await db.Appointments.AsNoTracking().Where(x => x.StaffId.HasValue && staffIds.Contains(x.StaffId.Value) && x.AppointmentDate == date
            && (x.Status == AppointmentStatus.AwaitingDeposit || x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed || x.Status == AppointmentStatus.InProgress)).ToListAsync(ct);
        var customerAppointments = customerId.HasValue
            ? await db.Appointments.AsNoTracking().Where(x => x.CustomerId == customerId.Value && x.AppointmentDate == date
                && (x.Status == AppointmentStatus.AwaitingDeposit || x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed || x.Status == AppointmentStatus.InProgress)).ToListAsync(ct)
            : [];
        var slots = new HashSet<TimeOnly>();
        foreach (var person in staff)
        {
            var windows = WorkingWindows(person.Id, schedules);
            foreach (var window in windows)
            {
                for (var slot = window.Start; slot.AddMinutes(duration) <= window.End; slot = slot.AddMinutes(SlotIntervalMinutes))
                {
                    var endTime = slot.AddMinutes(duration);
                    if (date == Today() && slot <= LocalNow()) continue;
                    if (timeOffs.Any(x => x.StaffId == person.Id && BookingRules.Overlaps(slot, endTime, x.StartTime, x.EndTime))) continue;
                    if (staffAppointments.Any(x => x.StaffId == person.Id && BookingRules.Overlaps(slot, endTime, x.StartTime, x.EndTime))) continue;
                    if (customerAppointments.Any(x => BookingRules.Overlaps(slot, endTime, x.StartTime, x.EndTime))) continue;
                    slots.Add(slot);
                }
            }
        }
        return Ok(new AvailableSlotsResponse(date, duration, slots.Order().Select(start => new BookingSlotDto(start, start.AddMinutes(duration))).ToList()));
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpPost("api/appointments")]
    public async Task<ActionResult<AppointmentBookingDto>> Create(CreateAppointmentRequest request, CancellationToken ct)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!ValidDate(request.AppointmentDate) || request.Note?.Length > 500) return BadRequest(new { code = "INVALID_BOOKING_REQUEST" });
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var service = await db.Services.FirstOrDefaultAsync(x => x.Id == request.ServiceId && x.IsActive && x.IsBookable, ct);
            var design = await db.NailDesigns.FirstOrDefaultAsync(x => x.Id == request.NailDesignId && x.Status == UserStatus.Active && x.IsBookable, ct);
            if (service is null || design is null) return BadRequest(new { code = "SERVICE_OR_DESIGN_UNAVAILABLE" });
            if (!await db.ServiceNailDesigns.AnyAsync(x => x.ServiceId == service.Id && x.NailDesignId == design.Id, ct)) return BadRequest(new { code = "INCOMPATIBLE_SERVICE_DESIGN" });
            var duration = BookingRules.TotalDurationMinutes(service.DurationMinutes, design.AdditionalDurationMinutes);
            var endTime = request.StartTime.AddMinutes(duration);
            if (await CustomerHasOverlap(customerId, request.AppointmentDate, request.StartTime, endTime, null, ct))
                return Conflict(new { code = "CUSTOMER_SLOT_CONFLICT", message = "You already have an appointment in this time slot." });
            var staff = await EligibleStaff(service.Id, request.StaffId, ct);
            var selected = await FirstAvailableStaff(staff, request.AppointmentDate, request.StartTime, endTime, null, ct);
            if (selected is null) return Conflict(new { code = "SLOT_UNAVAILABLE", message = "This time slot is no longer available." });
            var totalPrice = BookingRules.TotalPrice(service.Price, design.ExtraPrice);
            var appointment = new Appointment { CustomerId = customerId, StaffId = selected.Id, NailDesignId = design.Id, AppointmentDate = request.AppointmentDate, StartTime = request.StartTime, EndTime = endTime, TotalPrice = totalPrice, DesignExtraPriceAtBooking = design.ExtraPrice, DesignAdditionalDurationAtBooking = design.AdditionalDurationMinutes, Status = AppointmentStatus.AwaitingDeposit, Note = request.Note?.Trim(), AppointmentServices = [new AppointmentService { ServiceId = service.Id, PriceAtBooking = service.Price, DurationAtBooking = service.DurationMinutes }], DepositPayment = new DepositPayment { Amount = DepositAmount(totalPrice), ExpiresAt = DateTime.UtcNow.AddMinutes(10) } };
            db.Appointments.Add(appointment);
            db.Notifications.Add(new Notification { UserId = customerId, AppointmentId = appointment.Id, Type = "DepositRequired", Title = "Cần thanh toán tiền cọc", Message = $"Vui lòng thanh toán cọc trong 10 phút để giữ lịch {request.AppointmentDate:dd/MM/yyyy} lúc {request.StartTime:HH\\:mm}.", Link = $"/deposit/{appointment.Id}" });
            db.Notifications.Add(new Notification { UserId = selected.Id, AppointmentId = appointment.Id, Type = "BookingAwaitingDeposit", Title = "Có lịch hẹn chờ tiền cọc", Message = $"{service.Name} đang giữ slot trong 10 phút chờ khách thanh toán cọc.", Link = "/staff/appointments" });
            var adminIds = await db.Users.Where(x => x.Role == UserRole.Admin && x.Status == UserStatus.Active).Select(x => x.Id).ToListAsync(ct);
            db.Notifications.AddRange(adminIds.Select(id => new Notification { UserId = id, AppointmentId = appointment.Id, Type = "BookingAwaitingDeposit", Title = "Có lịch hẹn mới", Message = $"{service.Name} đang giữ chỗ 10 phút chờ khách đặt cọc, nhân viên {selected.FullName}.", Link = "/staff/appointments" }));
            await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
            return CreatedAtAction(nameof(Get), new { id = appointment.Id }, new AppointmentBookingDto(appointment.Id, selected.Id, service.Id, design.Id, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime, service.Price, design.ExtraPrice, appointment.TotalPrice, appointment.Status.ToString()));
        }
        catch (Exception exception) when (IsBookingConcurrencyFailure(exception))
        {
            return Conflict(new { code = "SLOT_UNAVAILABLE", message = "This time slot is no longer available." });
        }
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("api/appointments")]
    public async Task<ActionResult<IReadOnlyList<CustomerAppointmentDto>>> GetMine(CancellationToken ct)
    {
        var customerId = CurrentCustomerId();
        var appointments = await db.Appointments.AsNoTracking()
            .Include(x => x.AppointmentServices).ThenInclude(x => x.Service)
            .Include(x => x.NailDesign)
            .Include(x => x.Staff)
            .Include(x => x.Reviews)
            .Include(x => x.DepositPayment)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.StartTime)
            .ToListAsync(ct);
        var result = appointments.Select(x =>
        {
            var service = x.AppointmentServices.FirstOrDefault();
            return new CustomerAppointmentDto(
                x.Id,
                $"APT-{x.AppointmentDate:yyyyMMdd}-{x.Id.ToString()[..6].ToUpperInvariant()}",
                x.AppointmentDate,
                x.StartTime,
                x.EndTime,
                x.Status.ToString(),
                service?.ServiceId ?? Guid.Empty,
                service?.Service.Name ?? "Dịch vụ làm nail",
                x.NailDesignId,
                x.NailDesign?.Name,
                x.StaffId,
                x.Staff?.FullName,
                service?.PriceAtBooking ?? 0,
                x.DesignExtraPriceAtBooking,
                x.TotalPrice,
                x.Note,
                x.CancellationReason,
                x.CreatedAt,
                x.AppointmentServices.Select(item => new AppointmentServiceReviewDto(
                    item.ServiceId,
                    item.Service.Name,
                    item.PriceAtBooking,
                    item.DurationAtBooking,
                    x.Reviews.Where(review => review.ServiceId == item.ServiceId)
                        .Select(review => new ReviewDto(review.Id, review.Rating, review.Comment, review.IsApproved, review.CreatedAt))
                        .FirstOrDefault())).ToList(),
                x.DepositPayment is null ? null : new DepositPaymentDto(x.DepositPayment.Id, x.DepositPayment.Amount, x.DepositPayment.Status.ToString(), x.DepositPayment.ExpiresAt, x.DepositPayment.ReceiptPath, "", ""));
        }).ToList();
        return Ok(result);
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("api/appointments/{id:guid}")]
    public async Task<ActionResult<AppointmentBookingDto>> Get(Guid id, CancellationToken ct)
    {
        var customerId = CurrentCustomerId();
        var appointment = await db.Appointments.AsNoTracking().Include(x => x.AppointmentServices)
            .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        return appointment is null ? NotFound() : Ok(ToDto(appointment));
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpPatch("api/appointments/{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentBookingDto>> Cancel(Guid id, CancelAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await CustomerAppointment(id, ct);
        if (appointment is null) return NotFound();
        if (!BookingRules.CanCustomerCancel(appointment.Status, HasStarted(appointment)))
            return Conflict(new { code = "APPOINTMENT_CANNOT_BE_CANCELLED" });
        if (request.Reason?.Length > 500) return BadRequest(new { code = "INVALID_CANCELLATION_REASON" });
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason?.Trim();
        if (appointment.StaffId.HasValue) db.Notifications.Add(new Notification { UserId = appointment.StaffId.Value, AppointmentId = appointment.Id, Type = "BookingCancelled", Title = "Khách đã hủy lịch", Message = $"Lịch ngày {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:HH\\:mm} đã được khách hủy.", Link = "/staff/appointments" });
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(appointment));
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpPatch("api/appointments/{id:guid}/reschedule")]
    public async Task<ActionResult<AppointmentBookingDto>> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await CustomerAppointment(id, ct);
        if (appointment is null) return NotFound();
        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed) || HasStarted(appointment) || !ValidDate(request.AppointmentDate))
            return Conflict(new { code = "APPOINTMENT_CANNOT_BE_RESCHEDULED" });
        var serviceEntry = appointment.AppointmentServices.SingleOrDefault();
        if (serviceEntry is null || appointment.NailDesignId is null) return Conflict(new { code = "INVALID_APPOINTMENT_DATA" });
        var stillCompatible = await db.Services.AnyAsync(x => x.Id == serviceEntry.ServiceId && x.IsActive && x.IsBookable, ct)
            && await db.NailDesigns.AnyAsync(x => x.Id == appointment.NailDesignId && x.Status == UserStatus.Active && x.IsBookable, ct)
            && await db.ServiceNailDesigns.AnyAsync(x => x.ServiceId == serviceEntry.ServiceId && x.NailDesignId == appointment.NailDesignId, ct);
        if (!stillCompatible) return Conflict(new { code = "SERVICE_OR_DESIGN_UNAVAILABLE" });
        var duration = BookingRules.TotalDurationMinutes(serviceEntry.DurationAtBooking, appointment.DesignAdditionalDurationAtBooking);
        var endTime = request.StartTime.AddMinutes(duration);
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            if (await CustomerHasOverlap(appointment.CustomerId, request.AppointmentDate, request.StartTime, endTime, appointment.Id, ct))
                return Conflict(new { code = "CUSTOMER_SLOT_CONFLICT", message = "You already have an appointment in this time slot." });
            var staff = await EligibleStaff(serviceEntry.ServiceId, request.StaffId, ct);
            var selected = await FirstAvailableStaff(staff, request.AppointmentDate, request.StartTime, endTime, appointment.Id, ct);
            if (selected is null) return Conflict(new { code = "SLOT_UNAVAILABLE" });
            appointment.StaffId = selected.Id;
            appointment.AppointmentDate = request.AppointmentDate;
            appointment.StartTime = request.StartTime;
            appointment.EndTime = endTime;
            db.Notifications.Add(new Notification { UserId = selected.Id, AppointmentId = appointment.Id, Type = "BookingRescheduled", Title = "Lịch hẹn đã được đổi", Message = $"Lịch mới: {request.AppointmentDate:dd/MM/yyyy} lúc {request.StartTime:HH\\:mm}. Lịch đang chờ xác nhận lại.", Link = "/staff/appointments" });
            appointment.Status = AppointmentStatus.Pending;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Ok(ToDto(appointment));
        }
        catch (Exception exception) when (IsBookingConcurrencyFailure(exception))
        {
            return Conflict(new { code = "SLOT_UNAVAILABLE", message = "This time slot is no longer available." });
        }
    }

    private Guid CurrentCustomerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Guid? TryCurrentCustomerId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId) && User.IsInRole("Customer") ? customerId : null;
    private bool ValidDate(DateOnly date) => date >= Today() && date <= Today().AddDays(MaxAdvanceDays);
    private bool HasStarted(Appointment appointment) => appointment.AppointmentDate < Today() || appointment.AppointmentDate == Today() && appointment.StartTime <= LocalNow();
    private async Task<List<User>> EligibleStaff(Guid serviceId, Guid? staffId, CancellationToken ct) => await db.Users.Where(x => x.Role == UserRole.Staff && x.Status == UserStatus.Active && x.StaffServices.Any(s => s.ServiceId == serviceId) && (!staffId.HasValue || x.Id == staffId.Value)).OrderBy(x => x.AppointmentsAsStaff.Count(a => a.Status == AppointmentStatus.AwaitingDeposit || a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.InProgress)).ToListAsync(ct);
    private async Task<User?> FirstAvailableStaff(IEnumerable<User> staff, DateOnly date, TimeOnly start, TimeOnly end, Guid? excludedAppointmentId, CancellationToken ct)
    {
        foreach (var person in staff)
            if (await IsAvailable(person.Id, date, start, end, excludedAppointmentId, ct)) return person;
        return null;
    }
    private async Task<bool> IsAvailable(Guid staffId, DateOnly date, TimeOnly start, TimeOnly end, Guid? excludedAppointmentId, CancellationToken ct)
    {
        if (end <= start || date == Today() && start <= LocalNow()) return false;
        var schedules = await db.StaffSchedules.AsNoTracking().Where(x => x.StaffId == staffId && x.WorkDate == date).ToListAsync(ct);
        var withinWorkingHours = WorkingWindows(staffId, schedules).Any(x => start >= x.Start && end <= x.End
            && (start.ToTimeSpan() - x.Start.ToTimeSpan()).TotalMinutes % SlotIntervalMinutes == 0);
        return withinWorkingHours
            && !await db.StaffTimeOffs.AnyAsync(x => x.StaffId == staffId && x.Date == date && start < x.EndTime && end > x.StartTime, ct)
            && !await db.Appointments.AnyAsync(x => x.Id != excludedAppointmentId && x.StaffId == staffId && x.AppointmentDate == date && (x.Status == AppointmentStatus.AwaitingDeposit || x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed || x.Status == AppointmentStatus.InProgress) && start < x.EndTime && end > x.StartTime, ct);
    }
    private Task<bool> CustomerHasOverlap(Guid customerId, DateOnly date, TimeOnly start, TimeOnly end, Guid? excludedAppointmentId, CancellationToken ct) =>
        db.Appointments.AnyAsync(x => x.Id != excludedAppointmentId && x.CustomerId == customerId && x.AppointmentDate == date
            && (x.Status == AppointmentStatus.AwaitingDeposit || x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed || x.Status == AppointmentStatus.InProgress)
            && start < x.EndTime && end > x.StartTime, ct);
    private IReadOnlyList<(TimeOnly Start, TimeOnly End)> WorkingWindows(Guid staffId, IReadOnlyCollection<StaffSchedule> schedules)
    {
        var staffSchedules = schedules.Where(x => x.StaffId == staffId).ToList();
        var configuredWorkingHours = staffSchedules.Where(x => x.Status == ScheduleStatus.Working).Select(x => (x.StartTime, x.EndTime)).ToList();
        if (configuredWorkingHours.Count > 0) return configuredWorkingHours;
        if (staffSchedules.Count > 0) return [];
        return [(SalonOpeningTime(), SalonClosingTime())];
    }
    private TimeOnly SalonOpeningTime() => TimeOnly.TryParse(configuration["Salon:BookingHours:Start"], out var value) ? value : new TimeOnly(9, 0);
    private TimeOnly SalonClosingTime() => TimeOnly.TryParse(configuration["Salon:BookingHours:End"], out var value) ? value : new TimeOnly(18, 0);
    private DateTime LocalDateTimeNow()
    {
        var timezoneId = configuration["Salon:Timezone"] ?? "Asia/Ho_Chi_Minh";
        TimeZoneInfo timezone;
        try { timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId); }
        catch (TimeZoneNotFoundException) { timezone = TimeZoneInfo.Local; }
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
    }
    private DateOnly Today() => DateOnly.FromDateTime(LocalDateTimeNow());
    private TimeOnly LocalNow() => TimeOnly.FromDateTime(LocalDateTimeNow());
    private static bool IsBookingConcurrencyFailure(Exception exception) => exception switch
    {
        PostgresException postgres when postgres.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.ExclusionViolation => true,
        DbUpdateException { InnerException: PostgresException postgres } when postgres.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.ExclusionViolation => true,
        _ => false
    };
    private decimal DepositPercent() => decimal.TryParse(configuration["Payment:DepositPercent"], out var percent) && percent is > 0 and <= 1 ? percent : 0.30m;
    private decimal DepositAmount(decimal totalVnd) => BookingRules.DepositAmount(totalVnd, DepositPercent());
    private async Task<Appointment?> CustomerAppointment(Guid id, CancellationToken ct) => await db.Appointments.Include(x => x.AppointmentServices).FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == CurrentCustomerId(), ct);
    private static AppointmentBookingDto ToDto(Appointment appointment)
    {
        var service = appointment.AppointmentServices.Single();
        return new AppointmentBookingDto(appointment.Id, appointment.StaffId ?? Guid.Empty, service.ServiceId, appointment.NailDesignId ?? Guid.Empty, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime, service.PriceAtBooking, appointment.DesignExtraPriceAtBooking, appointment.TotalPrice, appointment.Status.ToString());
    }
}
