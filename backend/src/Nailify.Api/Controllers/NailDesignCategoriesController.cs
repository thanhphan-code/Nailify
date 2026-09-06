using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.NailDesigns;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/nail-design-categories")]
public class NailDesignCategoriesController(NailifyDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<NailDesignCategoryListResponse>> Get(CancellationToken ct)
    {
        var categories = await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, DesignCount = x.NailDesigns.Count(d => d.Status == UserStatus.Active) })
            .Where(x => x.DesignCount > 0).ToListAsync(ct);
        return Ok(new NailDesignCategoryListResponse(categories.Select(x => new NailDesignCategoryCountDto(x.Id, x.Name, x.Name.ToLowerInvariant().Replace(' ', '-'), x.DesignCount)).ToList()));
    }
}
