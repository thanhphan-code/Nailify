using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.NailDesigns;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/nail-designs")]
public class NailDesignsController(NailifyDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<NailDesignListResponse>> Get([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] string? category, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string sort = "newest", [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
    {
        if (search?.Length > 100 || minPrice < 0 || maxPrice < 0 || minPrice > maxPrice || sort is not ("recommended" or "newest" or "price-asc" or "price-desc" or "name-asc")) return BadRequest(new { code = "INVALID_SEARCH_FILTER", message = "One or more filters are invalid." });
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 50);
        var query = db.NailDesigns.AsNoTracking().Include(x => x.Category).Where(x => x.Status == UserStatus.Active && x.Category.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => EF.Functions.ILike(x.Name, $"%{term}%")); }
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        else if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category.Name == category || EF.Functions.ILike(x.Category.Name, category));
        if (minPrice.HasValue) query = query.Where(x => x.ExtraPrice >= minPrice); if (maxPrice.HasValue) query = query.Where(x => x.ExtraPrice <= maxPrice);
        query = sort switch { "recommended" => query.OrderByDescending(x => x.IsFeatured).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Name), "newest" => query.OrderByDescending(x => x.CreatedAt), "price-asc" => query.OrderBy(x => x.ExtraPrice), "price-desc" => query.OrderByDescending(x => x.ExtraPrice), _ => query.OrderBy(x => x.Name) };
        var total = await query.CountAsync(ct);
        var favoriteIds = await GetFavoriteIds(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new { x.Id, x.Name, x.ImageUrl, x.ExtraPrice, x.AdditionalDurationMinutes, x.IsBookable, CategoryId = x.Category.Id, CategoryName = x.Category.Name }).ToListAsync(ct);
        return Ok(new NailDesignListResponse(items.Select(x => new NailDesignListItemDto(x.Id, x.Name, x.ImageUrl, new NailDesignCategoryDto(x.CategoryId, x.CategoryName, Slug(x.CategoryName)), x.ExtraPrice, x.AdditionalDurationMinutes, x.IsBookable, favoriteIds.Contains(x.Id))).ToList(), new PaginationDto(page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize), page * pageSize < total, page > 1)));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NailDesignDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var design = await db.NailDesigns.AsNoTracking().Include(x => x.Category).Include(x => x.ServiceNailDesigns).ThenInclude(x => x.Service).FirstOrDefaultAsync(x => x.Id == id && x.Status == UserStatus.Active && x.Category.IsActive, ct);
        if (design is null) return NotFound(new { code = "DESIGN_NOT_FOUND", message = "Nail design was not found." });
        var favorites = await GetFavoriteIds(ct);
        var services = design.ServiceNailDesigns.Where(x => x.Service.IsActive && x.Service.IsBookable).Select(x => new CompatibleServiceDto(x.ServiceId, x.Service.Name, x.Service.Price, x.Service.DurationMinutes, x.Service.Price + design.ExtraPrice)).ToList();
        return Ok(new NailDesignDetailDto(design.Id, design.Name, design.ImageUrl, new NailDesignCategoryDto(design.Category.Id, design.Category.Name, Slug(design.Category.Name)), design.ExtraPrice, design.AdditionalDurationMinutes, design.IsBookable, favorites.Contains(design.Id), services));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/services")]
    public async Task<ActionResult<IReadOnlyList<CompatibleServiceDto>>> GetCompatibleServices(Guid id, CancellationToken ct)
    {
        var design = await db.NailDesigns.AsNoTracking().Include(x => x.ServiceNailDesigns).ThenInclude(x => x.Service)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == UserStatus.Active, ct);
        if (design is null) return NotFound(new { code = "DESIGN_NOT_FOUND" });
        return Ok(design.ServiceNailDesigns.Where(x => x.Service.IsActive && x.Service.IsBookable)
            .Select(x => new CompatibleServiceDto(x.ServiceId, x.Service.Name, x.Service.Price, x.Service.DurationMinutes, x.Service.Price + design.ExtraPrice)).ToList());
    }

    private async Task<HashSet<Guid>> GetFavoriteIds(CancellationToken ct)
    {
        var customerId = CurrentCustomerId(); if (customerId is null) return [];
        return (await db.CustomerFavoriteDesigns.AsNoTracking().Where(x => x.CustomerId == customerId).Select(x => x.NailDesignId).ToListAsync(ct)).ToHashSet();
    }
    private Guid? CurrentCustomerId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && User.IsInRole(UserRole.Customer.ToString()) ? id : null;
    private static string Slug(string value) => value.Trim().ToLowerInvariant().Replace(' ', '-');
}
