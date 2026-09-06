import { apiClient } from "@/services/apiClient";

export interface SystemNotification { id: string; type: string; title: string; message: string; link?: string | null; isRead: boolean; createdAt: string; }
export interface NotificationList { unreadCount: number; items: SystemNotification[]; }
export async function getNotifications() { const { data } = await apiClient.get<NotificationList>("/notifications"); return data; }
export async function markNotificationRead(id: string) { await apiClient.patch(`/notifications/${id}/read`); }
export async function markAllNotificationsRead() { await apiClient.patch("/notifications/read-all"); }
