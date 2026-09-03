using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.Services;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(NailifyDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceListResponse>> Get([FromQuery] string? category, [FromQuery] string? search, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? minDuration, [FromQuery] int? maxDuration, [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Services.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ServiceCategory>(category, true, out var parsed)) query = query.Where(x => x.Category == parsed);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search.Trim()}%"));
        if (minPrice.HasValue) query = query.Where(x => x.Price >= minPrice.Value); if (maxPrice.HasValue) query = query.Where(x => x.Price <= maxPrice.Value);
        if (minDuration.HasValue) query = query.Where(x => x.DurationMinutes >= minDuration.Value); if (maxDuration.HasValue) query = query.Where(x => x.DurationMinutes <= maxDuration.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Category).ThenBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(x => ToDto(x)).ToListAsync(ct);
        return Ok(new ServiceListResponse(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id, CancellationToken ct) => await db.Services.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct) is { } service ? Ok(ToDto(service)) : NotFound();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceRequest request, CancellationToken ct)
    {
        var category = ParseCategory(request.Category); if (category is null) return BadRequest(new { title = "Invalid category." });
        if (!IsValid(request.Name, request.Price, request.DurationMinutes)) return ValidationProblem();
        if (await db.Services.AnyAsync(x => x.Category == category && x.Name == request.Name.Trim(), ct)) return Conflict(new { title = "A service with this name already exists in this category." });
        var service = new Service { Name = request.Name.Trim(), Category = category.Value, Price = request.Price, DurationMinutes = request.DurationMinutes, Description = request.Description?.Trim(), ImageUrl = request.ImageUrl?.Trim() };
        db.Services.Add(service); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(GetById), new { id = service.Id }, ToDto(service));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Update(Guid id, UpdateServiceRequest request, CancellationToken ct)
    {
        var service = await db.Services.FindAsync([id], ct); if (service is null) return NotFound(); var category = ParseCategory(request.Category);
        if (category is null || !IsValid(request.Name, request.Price, request.DurationMinutes)) return ValidationProblem();
        if (await db.Services.AnyAsync(x => x.Id != id && x.Category == category && x.Name == request.Name.Trim(), ct)) return Conflict(new { title = "A service with this name already exists in this category." });
        service.Name = request.Name.Trim(); service.Category = category.Value; service.Price = request.Price; service.DurationMinutes = request.DurationMinutes; service.Description = request.Description?.Trim(); service.ImageUrl = request.ImageUrl?.Trim(); service.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ToDto(service));
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Toggle(Guid id, CancellationToken ct) { var service = await db.Services.FindAsync([id], ct); if (service is null) return NotFound(); service.IsActive = !service.IsActive; service.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Ok(ToDto(service)); }

    [HttpGet("reports/top")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<TopServiceDto>>> Top([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var query = db.AppointmentServices.AsNoTracking().Include(x => x.Appointment).Include(x => x.Service).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.Appointment.AppointmentDate >= from); if (to.HasValue) query = query.Where(x => x.Appointment.AppointmentDate <= to);
        return Ok(await query.GroupBy(x => new { x.ServiceId, x.Service.Name }).OrderByDescending(x => x.Sum(y => y.PriceAtBooking)).Select(x => new TopServiceDto(x.Key.ServiceId, x.Key.Name, x.Sum(y => y.PriceAtBooking), x.Count())).Take(10).ToListAsync(ct));
    }
    private static ServiceCategory? ParseCategory(string value) => Enum.TryParse<ServiceCategory>(value, true, out var category) ? category : null;
    private static bool IsValid(string name, decimal price, int duration) => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 150 && price >= 0 && duration > 0;
    private static ServiceDto ToDto(Service x) => new(x.Id, x.Name, x.Category.ToString(), x.Price, x.DurationMinutes, x.Description, x.ImageUrl, x.IsActive);
}
