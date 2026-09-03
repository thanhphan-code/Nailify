using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Navigation
    public StaffProfile? StaffProfile { get; set; }
    public ICollection<StaffSchedule> StaffSchedules { get; set; } = new List<StaffSchedule>();
    public ICollection<Appointment> AppointmentsAsCustomer { get; set; } = new List<Appointment>();
    public ICollection<Appointment> AppointmentsAsStaff { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
