import { useEffect, useRef, useState } from "react";
import type { AxiosError } from "axios";
import { changePassword } from "@/features/auth/api";
import { useAuth } from "@/hooks/useAuth";

export default function AccountMenu() {
  const { user, logout } = useAuth();
  const displayName = user?.fullName?.trim() || "Khách hàng";
  const roleLabel = user?.role === "Staff" ? "Nhân viên" : user?.role === "Admin" ? "Quản trị viên" : "Khách hàng";
  const rootRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const [changing, setChanging] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const close = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", close);
    return () => document.removeEventListener("mousedown", close);
  }, []);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(""); setMessage("");
    if (newPassword !== confirmPassword) { setError("Xác nhận mật khẩu mới không khớp."); return; }
    setChanging(true);
    try {
      await changePassword(currentPassword, newPassword);
      setMessage("Đổi mật khẩu thành công.");
      setCurrentPassword(""); setNewPassword(""); setConfirmPassword("");
    } catch (exception) {
      const axiosError = exception as AxiosError<{ title?: string }>;
      setError(axiosError.response?.data?.title ?? "Không thể đổi mật khẩu. Vui lòng thử lại.");
    } finally { setChanging(false); }
  };

  return <div className="account-menu" ref={rootRef}>
    <button type="button" className="account-menu-trigger" onClick={() => setOpen((value) => !value)} aria-expanded={open} aria-label="Mở menu tài khoản">
      <span className="home-avatar" aria-hidden="true">{displayName[0].toUpperCase()}</span>
      <span className="home-user"><strong>{displayName}</strong><small>{roleLabel}</small></span>
      <span className="account-chevron" aria-hidden="true">⌄</span>
    </button>
    {open && <section className="account-popover">
      <header><strong>{displayName}</strong><small>{user?.email}</small></header>
      <button type="button" onClick={() => { setOpen(false); void logout(); }}>Đăng xuất</button>
      <details><summary>Đổi mật khẩu</summary><form onSubmit={(event) => void submit(event)}>
        <label>Mật khẩu hiện tại<input type="password" autoComplete="current-password" required value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} /></label>
        <label>Mật khẩu mới<input type="password" autoComplete="new-password" required minLength={8} value={newPassword} onChange={(event) => setNewPassword(event.target.value)} /></label>
        <label>Xác nhận mật khẩu<input type="password" autoComplete="new-password" required minLength={8} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} /></label>
        <small>Ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.</small>
        {error && <p className="account-error" role="alert">{error}</p>}{message && <p className="account-success" role="status">{message}</p>}
        <button type="submit" disabled={changing}>{changing ? "Đang cập nhật…" : "Cập nhật mật khẩu"}</button>
      </form></details>
    </section>}
  </div>;
}
