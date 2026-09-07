import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getNotifications, markAllNotificationsRead, markNotificationRead, type NotificationList } from "@/features/notifications/api";
import { useAuthStore } from "@/store/authStore";

export default function NotificationCenter() {
  const navigate = useNavigate();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const [open, setOpen] = useState(false);
  const [data, setData] = useState<NotificationList>({ unreadCount: 0, items: [] });
  const [error, setError] = useState(false);

  const load = async () => {
    try { setData(await getNotifications()); setError(false); }
    catch { setError(true); }
  };

  useEffect(() => {
    if (!isAuthenticated) return;
    void load();
    const timer = window.setInterval(() => void load(), 30_000);
    return () => window.clearInterval(timer);
  }, [isAuthenticated]);

  if (!isAuthenticated) return null;
  return (
    <aside className="notification-center">
      <button type="button" className="notification-trigger" aria-label={`Thông báo, ${data.unreadCount} chưa đọc`} aria-expanded={open} onClick={() => setOpen((value) => !value)}>
        <span aria-hidden="true">♢</span>
        {data.unreadCount > 0 && <b>{data.unreadCount > 9 ? "9+" : data.unreadCount}</b>}
      </button>
      {open && <section className="notification-panel" aria-label="Thông báo hệ thống">
        <header><div><strong>Thông báo</strong><p>Cập nhật lịch hẹn từ hệ thống</p></div>{data.unreadCount > 0 && <button type="button" onClick={async () => { await markAllNotificationsRead(); await load(); }}>Đọc tất cả</button>}</header>
        {error ? <p className="notification-state">Không thể tải thông báo.</p> : data.items.length === 0 ? <p className="notification-state">Bạn chưa có thông báo mới.</p> : <div className="notification-list">{data.items.map((item) => <button type="button" key={`${item.type}-${item.id}`} className={item.isRead ? "is-read" : ""} onClick={async () => { if (!item.isRead && item.type !== "BookingOverdue") await markNotificationRead(item.id); setOpen(false); if (item.link) navigate(item.link); await load(); }}><i aria-hidden="true" /><span><strong>{item.title}</strong><small>{item.message}</small><time>{new Date(item.createdAt).toLocaleString("vi-VN")}</time></span></button>)}</div>}
      </section>}
    </aside>
  );
}
