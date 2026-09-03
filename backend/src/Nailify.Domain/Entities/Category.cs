using Nailify.Domain.Common;

namespace Nailify.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<NailDesign> NailDesigns { get; set; } = new List<NailDesign>();
}
