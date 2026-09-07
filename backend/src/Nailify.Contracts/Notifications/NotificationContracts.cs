namespace Nailify.Contracts.Notifications;

public sealed record NotificationDto(Guid Id, string Type, string Title, string Message, string? Link, bool IsRead, DateTime CreatedAt);
public sealed record NotificationListResponse(int UnreadCount, IReadOnlyList<NotificationDto> Items);
