using Nailify.Domain.Common;
using Nailify.Domain.Enums;

namespace Nailify.Domain.Entities;

// SRS 11.3 + BR-011 - dung de tinh Available Time khi dat lich
public class StaffSchedule : BaseEntity
{
    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;

    public DateOnly WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Working;
}
