using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Authorize(Policy = "CustomerOnly")]
[Route("api/customers/me/favorite-designs")]
public class CustomerFavoritesController(NailifyDbContext db) : ControllerBase
{
    [HttpPost("{designId:guid}")]
    public async Task<IActionResult> Add(Guid designId, CancellationToken ct)
    {
        var customerId = CustomerId();
        var designExists = await db.NailDesigns.AnyAsync(x => x.Id == designId && x.Status == UserStatus.Active, ct);
        if (!designExists) return NotFound(new { code = "DESIGN_NOT_FOUND", message = "Nail design was not found." });
        if (!await db.CustomerFavoriteDesigns.AnyAsync(x => x.CustomerId == customerId && x.NailDesignId == designId, ct))
        {
            db.CustomerFavoriteDesigns.Add(new CustomerFavoriteDesign { CustomerId = customerId, NailDesignId = designId });
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    [HttpDelete("{designId:guid}")]
    public async Task<IActionResult> Remove(Guid designId, CancellationToken ct)
    {
        var favorite = await db.CustomerFavoriteDesigns.FindAsync([CustomerId(), designId], ct);
        if (favorite is not null) { db.CustomerFavoriteDesigns.Remove(favorite); await db.SaveChangesAsync(ct); }
        return NoContent();
    }

    private Guid CustomerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
