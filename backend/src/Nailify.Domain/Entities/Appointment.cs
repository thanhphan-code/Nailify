using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

// Trung tam nghiep vu cua he thong - xem SRS muc 1.3, 7, 8
public class Appointment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid? StaffId { get; set; } // nullable - BR-010, staff la optional
    public User? Staff { get; set; }

    public Guid? NailDesignId { get; set; }
    public NailDesign? NailDesign { get; set; }
    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; } // tinh theo BR-004

    public decimal TotalPrice { get; set; } // tinh theo BR-008
    // Snapshot of the design surcharge at checkout; later catalogue edits must not change booking history.
    public decimal DesignExtraPriceAtBooking { get; set; }
    public int DesignAdditionalDurationAtBooking { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Note { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public DepositPayment? DepositPayment { get; set; }
}
