namespace Nailify.Contracts.Services;

public sealed record ServiceDto(Guid Id, string Name, string Category, decimal Price, int DurationMinutes, string? Description, string? ImageUrl, bool IsActive);
public sealed record ServiceListResponse(IReadOnlyList<ServiceDto> Items, int TotalCount, int Page, int PageSize);
public sealed record CreateServiceRequest(string Name, string Category, decimal Price, int DurationMinutes, string? Description, string? ImageUrl);
public sealed record UpdateServiceRequest(string Name, string Category, decimal Price, int DurationMinutes, string? Description, string? ImageUrl);
public sealed record TopServiceDto(Guid ServiceId, string Name, decimal Revenue, int BookingCount);
