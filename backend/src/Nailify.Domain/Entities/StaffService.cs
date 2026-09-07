namespace Nailify.Domain.Entities;

public class StaffService
{
    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
