using Nailify.Domain.Enums;

namespace Nailify.Domain.Booking;

public static class BookingRules
{
    public static bool BlocksTime(AppointmentStatus status) =>
        status is AppointmentStatus.AwaitingDeposit or AppointmentStatus.Pending or AppointmentStatus.Confirmed or AppointmentStatus.InProgress;

    public static bool Overlaps(TimeOnly firstStart, TimeOnly firstEnd, TimeOnly secondStart, TimeOnly secondEnd) =>
        firstStart < secondEnd && firstEnd > secondStart;

    public static int TotalDurationMinutes(int serviceDurationMinutes, int designAdditionalDurationMinutes)
    {
        if (serviceDurationMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(serviceDurationMinutes));
        if (designAdditionalDurationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(designAdditionalDurationMinutes));
        return checked(serviceDurationMinutes + designAdditionalDurationMinutes);
    }

    public static decimal TotalPrice(decimal servicePrice, decimal designExtraPrice)
    {
        if (servicePrice < 0) throw new ArgumentOutOfRangeException(nameof(servicePrice));
        if (designExtraPrice < 0) throw new ArgumentOutOfRangeException(nameof(designExtraPrice));
        return servicePrice + designExtraPrice;
    }

    public static bool CanCustomerCancel(AppointmentStatus status, bool hasStarted) =>
        !hasStarted && status is AppointmentStatus.AwaitingDeposit or AppointmentStatus.Pending or AppointmentStatus.Confirmed;

    public static decimal DepositAmount(decimal totalPrice, decimal depositPercent)
    {
        if (totalPrice < 0) throw new ArgumentOutOfRangeException(nameof(totalPrice));
        if (depositPercent is <= 0 or > 1) throw new ArgumentOutOfRangeException(nameof(depositPercent));
        return decimal.Round(totalPrice * depositPercent, 0, MidpointRounding.AwayFromZero);
    }
}
