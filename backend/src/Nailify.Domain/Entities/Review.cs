using Nailify.Domain.Common;

namespace Nailify.Domain.Entities;

public class Review : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int Rating { get; set; } // 1-5, BR-005
    public string? Comment { get; set; }
}
