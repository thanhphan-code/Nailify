namespace Nailify.Domain.Entities;

/// <summary>
/// Defines a valid Service - NailDesign combination (BR-009).
/// </summary>
public class ServiceNailDesign
{
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public Guid NailDesignId { get; set; }
    public NailDesign NailDesign { get; set; } = null!;
}
