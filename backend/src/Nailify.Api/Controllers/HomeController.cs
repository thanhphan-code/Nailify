using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Nailify.Contracts.Home;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController(NailifyDbContext db, IMemoryCache cache, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<HomeResponse>> Get([FromQuery] int featuredDesignLimit = 4, [FromQuery] int serviceLimit = 4, [FromQuery] int reviewLimit = 3, CancellationToken ct = default)
    {
        featuredDesignLimit = Math.Clamp(featuredDesignLimit, 1, 12);
        serviceLimit = Math.Clamp(serviceLimit, 1, 12);
        reviewLimit = Math.Clamp(reviewLimit, 1, 12);
        var customerId = await GetActiveCustomerId(ct);
        var publicData = await cache.GetOrCreateAsync($"home-public:{featuredDesignLimit}:{serviceLimit}:{reviewLimit}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            var categories = await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
                .Select(x => new HomeCategoryDto(x.Id, x.Name, Slug(x.Name), x.NailDesigns.Count(d => d.Status == UserStatus.Active))).ToListAsync(ct);
            var designs = await db.NailDesigns.AsNoTracking().Include(x => x.Category).Where(x => x.Status == UserStatus.Active && x.IsFeatured && x.Category.IsActive && !string.IsNullOrWhiteSpace(x.ImageUrl))
                .OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreatedAt).Take(featuredDesignLimit)
                .Select(x => new FeaturedDesignDto(x.Id, x.Name, x.ImageUrl, x.Category.Name, x.ExtraPrice, x.AdditionalDurationMinutes, false)).ToListAsync(ct);
            var services = await db.Services.AsNoTracking().Where(x => x.IsActive && x.IsBookable).OrderByDescending(x => x.IsFeatured).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Name).Take(serviceLimit)
                .Select(x => new PopularServiceDto(x.Id, x.Name, x.Description, x.DurationMinutes, x.Price, x.ImageUrl)).ToListAsync(ct);
            var reviewRows = await db.Reviews.AsNoTracking().Where(x => x.IsApproved && x.IsVisible && x.Appointment.Status == AppointmentStatus.Completed)
                .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.CreatedAt).Take(reviewLimit)
                .Select(x => new { x.Customer.FullName, x.Rating, x.Comment, ServiceName = x.Service.Name, x.CreatedAt }).ToListAsync(ct);
            var reviews = reviewRows.Select(x => new FeaturedReviewDto(ShortName(x.FullName), x.Rating, x.Comment, x.ServiceName, x.CreatedAt)).ToList();
            return new PublicHomeData(categories, designs, services, reviews);
        }) ?? throw new InvalidOperationException("Unable to load Home data.");
        var favorites = customerId is null ? new HashSet<Guid>() : (await db.CustomerFavoriteDesigns.AsNoTracking().Where(x => x.CustomerId == customerId).Select(x => x.NailDesignId).ToListAsync(ct)).ToHashSet();
        var upcoming = customerId is null ? null : await GetUpcoming(customerId.Value, ct);
        var banner = new HomeBannerDto(configuration["Home:Banner:Title"] ?? "Tìm mẫu nail tiếp theo của bạn", configuration["Home:Banner:Description"] ?? "Khám phá mẫu nail và đặt lịch trực tuyến chỉ trong vài phút.", configuration["Home:Banner:ImageUrl"], new HomeActionDto("Đặt lịch ngay", "/services"), new HomeActionDto("Xem mẫu nail", "/nail-designs"));
        var salon = new SalonDto(configuration["Salon:Name"] ?? "Nailify Salon", configuration["Salon:Phone"] ?? "", configuration["Salon:Address"] ?? "", configuration["Salon:Timezone"] ?? "Asia/Ho_Chi_Minh", configuration.GetSection("Salon:OpeningHours").Get<string[]>() ?? []);
        return Ok(new HomeResponse(banner, publicData.Categories, publicData.Designs.Select(x => x with { IsFavorite = favorites.Contains(x.Id) }).ToList(), publicData.Services, upcoming, publicData.Reviews, salon));
    }

    private async Task<Guid?> GetActiveCustomerId(CancellationToken ct)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var id)) return null;
        return await db.Users.AsNoTracking().AnyAsync(x => x.Id == id && x.Role == UserRole.Customer && x.Status == UserStatus.Active, ct) ? id : null;
    }
    private async Task<UpcomingAppointmentDto?> GetUpcoming(Guid customerId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return await db.Appointments.AsNoTracking().Where(x => x.CustomerId == customerId && (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed) && (x.AppointmentDate > today || x.AppointmentDate == today && x.StartTime > now)).OrderBy(x => x.AppointmentDate).ThenBy(x => x.StartTime)
            .Select(x => new UpcomingAppointmentDto(x.Id, $"APT-{x.AppointmentDate:yyyyMMdd}-{x.Id.ToString().Substring(0, 6).ToUpper()}", x.AppointmentDate.ToDateTime(x.StartTime, DateTimeKind.Utc), x.AppointmentDate.ToDateTime(x.EndTime, DateTimeKind.Utc), x.Status.ToString(), x.AppointmentServices.Select(s => s.Service.Name).FirstOrDefault() ?? "Dịch vụ nail", x.NailDesign == null ? null : x.NailDesign.Name, x.Staff == null ? null : x.Staff.FullName, x.TotalPrice)).FirstOrDefaultAsync(ct);
    }
    private static string ShortName(string fullName) { var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries); return parts.Length < 2 ? fullName : $"{parts[0]} {parts[^1][0]}."; }
    private static string Slug(string value) => value.Trim().ToLowerInvariant().Replace(' ', '-');
    private sealed record PublicHomeData(IReadOnlyList<HomeCategoryDto> Categories, IReadOnlyList<FeaturedDesignDto> Designs, IReadOnlyList<PopularServiceDto> Services, IReadOnlyList<FeaturedReviewDto> Reviews);
}
