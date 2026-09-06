namespace Nailify.Contracts.Home;

public sealed record HomeResponse(
    HomeBannerDto Banner,
    IReadOnlyList<HomeCategoryDto> Categories,
    IReadOnlyList<FeaturedDesignDto> FeaturedDesigns,
    IReadOnlyList<PopularServiceDto> PopularServices,
    UpcomingAppointmentDto? UpcomingAppointment,
    IReadOnlyList<FeaturedReviewDto> FeaturedReviews,
    SalonDto Salon);

public sealed record HomeBannerDto(string Title, string Description, string? ImageUrl, HomeActionDto PrimaryAction, HomeActionDto SecondaryAction);
public sealed record HomeActionDto(string Label, string Url);
public sealed record HomeCategoryDto(Guid Id, string Name, string Slug, int DesignCount);
public sealed record FeaturedDesignDto(Guid Id, string Name, string ImageUrl, string CategoryName, decimal AdditionalPrice, int AdditionalDurationMinutes, bool IsFavorite);
public sealed record PopularServiceDto(Guid Id, string Name, string? Description, int DurationMinutes, decimal BasePrice, string? ImageUrl);
public sealed record UpcomingAppointmentDto(Guid Id, string AppointmentCode, DateTime StartTime, DateTime EndTime, string Status, string ServiceName, string? DesignName, string? StaffName, decimal EstimatedTotal);
public sealed record FeaturedReviewDto(string CustomerName, int Rating, string? Comment, string ServiceName, DateTime CreatedAt);
public sealed record SalonDto(string Name, string Phone, string Address, string Timezone, IReadOnlyList<string> OpeningHours);
