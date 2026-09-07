namespace Nailify.Domain.Entities;

public class CustomerFavoriteDesign
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid NailDesignId { get; set; }
    public NailDesign NailDesign { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
