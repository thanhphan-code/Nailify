namespace Nailify.Domain.Enums;

// See SRS section 7 - Appointment status transition table
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    AwaitingDeposit
}
