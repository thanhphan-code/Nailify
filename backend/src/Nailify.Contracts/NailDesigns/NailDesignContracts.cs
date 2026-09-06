namespace Nailify.Contracts.NailDesigns;

public sealed record NailDesignListResponse(IReadOnlyList<NailDesignListItemDto> Items, PaginationDto Pagination);
public sealed record NailDesignListItemDto(Guid Id, string Name, string ImageUrl, NailDesignCategoryDto Category, decimal AdditionalPrice, int AdditionalDurationMinutes, bool IsBookable, bool IsFavorite);
public sealed record NailDesignCategoryDto(Guid Id, string Name, string Slug);
public sealed record PaginationDto(int Page, int PageSize, int TotalItems, int TotalPages, bool HasNextPage, bool HasPreviousPage);
public sealed record NailDesignDetailDto(Guid Id, string Name, string ImageUrl, NailDesignCategoryDto Category, decimal AdditionalPrice, int AdditionalDurationMinutes, bool IsBookable, bool IsFavorite, IReadOnlyList<CompatibleServiceDto> CompatibleServices);
public sealed record CompatibleServiceDto(Guid ServiceId, string ServiceName, decimal BasePrice, int TotalDurationMinutes, decimal EstimatedTotal);
public sealed record CompatibleNailDesignDto(Guid NailDesignId, string Name, string ImageUrl, NailDesignCategoryDto Category, decimal ExtraPrice, decimal EstimatedTotal);
public sealed record NailDesignCategoryListResponse(IReadOnlyList<NailDesignCategoryCountDto> Items);
public sealed record NailDesignCategoryCountDto(Guid Id, string Name, string Slug, int DesignCount);
