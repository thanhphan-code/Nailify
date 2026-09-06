using Nailify.Domain.Common;

namespace Nailify.Domain.Entities;

public class StaffTimeOff : BaseEntity
{
    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
}
