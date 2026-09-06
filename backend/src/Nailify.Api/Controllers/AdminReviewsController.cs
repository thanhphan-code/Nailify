using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/reviews")]
public class AdminReviewsController(NailifyDbContext db) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<ActionResult> Pending(CancellationToken ct) => Ok(await db.Reviews.AsNoTracking()
        .Where(x => !x.IsApproved && x.IsVisible)
        .OrderBy(x => x.CreatedAt)
        .Select(x => new { x.Id, x.Rating, x.Comment, x.CreatedAt, CustomerName = x.Customer.FullName, ServiceName = x.Service.Name })
        .ToListAsync(ct));

    [HttpPatch("{id:guid}/approve")]
    public async Task<ActionResult> Approve(Guid id, CancellationToken ct)
    {
        var review = await db.Reviews.FindAsync([id], ct);
        if (review is null) return NotFound();
        review.IsApproved = true;
        review.IsVisible = true;
        review.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/hide")]
    public async Task<ActionResult> Hide(Guid id, CancellationToken ct)
    {
        var review = await db.Reviews.FindAsync([id], ct);
        if (review is null) return NotFound();
        review.IsVisible = false;
        review.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
