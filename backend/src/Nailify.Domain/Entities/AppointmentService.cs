namespace Nailify.Domain.Entities;

public class AppointmentService
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public decimal PriceAtBooking { get; set; }
    public int DurationAtBooking { get; set; }
}
