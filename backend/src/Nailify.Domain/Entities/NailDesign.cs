using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

public class NailDesign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal ExtraPrice { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active; // reuse Active/Inactive semantics

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ServiceNailDesign> ServiceNailDesigns { get; set; } = new List<ServiceNailDesign>();
}
