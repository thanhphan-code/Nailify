using Nailify.Domain.Common;

namespace Nailify.Domain.Entities;

// SRS 11.2 - tach rieng khoi User, quan he 1:1
public class StaffProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Specialty { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}
