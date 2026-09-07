namespace Nailify.Contracts.Bookings;

public sealed record BookingSlotDto(TimeOnly StartTime, TimeOnly EndTime);
public sealed record AvailableSlotsResponse(DateOnly Date, int DurationMinutes, IReadOnlyList<BookingSlotDto> AvailableSlots);
public sealed record StaffBookingDto(Guid Id, string FullName, string? Specialty);
public sealed record CreateAppointmentRequest(Guid ServiceId, Guid NailDesignId, Guid? StaffId, DateOnly AppointmentDate, TimeOnly StartTime, string? Note);
public sealed record AppointmentBookingDto(Guid Id, Guid StaffId, Guid ServiceId, Guid NailDesignId, DateOnly AppointmentDate, TimeOnly StartTime, TimeOnly EndTime, decimal ServicePrice, decimal DesignExtraPrice, decimal TotalPrice, string Status, DepositPaymentDto? Deposit = null);
public sealed record DepositPaymentDto(Guid Id, decimal Amount, string Status, DateTime ExpiresAt, string? ReceiptUrl, string QrUrl, string TransferContent, string? CheckoutUrl = null);
public sealed record CustomerAppointmentDto(
    Guid Id,
    string AppointmentCode,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    Guid ServiceId,
    string ServiceName,
    Guid? NailDesignId,
    string? NailDesignName,
    Guid? StaffId,
    string? StaffName,
    decimal ServicePrice,
    decimal DesignExtraPrice,
    decimal TotalPrice,
    string? Note,
    string? CancellationReason,
    DateTime CreatedAt,
    IReadOnlyList<AppointmentServiceReviewDto>? Services = null,
    DepositPaymentDto? Deposit = null);
public sealed record AppointmentServiceReviewDto(Guid ServiceId, string ServiceName, decimal Price, int DurationMinutes, ReviewDto? Review);
public sealed record ReviewDto(Guid Id, int Rating, string? Comment, bool IsApproved, DateTime CreatedAt);
public sealed record CreateReviewRequest(int Rating, string? Comment);
public sealed record CancelAppointmentRequest(string? Reason);
public sealed record RescheduleAppointmentRequest(Guid? StaffId, DateOnly AppointmentDate, TimeOnly StartTime);
public sealed record UpdateAppointmentStatusDto(Guid Id, string Status, DateTime UpdatedAt);
