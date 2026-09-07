using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBookable { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    public ICollection<ServiceNailDesign> ServiceNailDesigns { get; set; } = new List<ServiceNailDesign>();
    public ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
