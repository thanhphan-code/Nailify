import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getStaffAppointments, updateAppointmentStatus } from "@/features/staffAppointments/api";
import type { CustomerAppointment } from "@/features/bookings/api";
import { appointmentStatusClasses, appointmentStatusLabels, depositStatusLabels } from "@/features/bookings/status";
import { useAuth } from "@/hooks/useAuth";

const nextAction: Record<string, { key: "confirm" | "start" | "complete"; label: string }> = {
  Pending: { key: "confirm", label: "Xác nhận lịch đã đổi" },
  Confirmed: { key: "start", label: "Bắt đầu dịch vụ" },
  InProgress: { key: "complete", label: "Hoàn thành" },
};

const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value);

export default function StaffAppointmentsPage() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [items, setItems] = useState<CustomerAppointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = async () => {
    try { setItems(await getStaffAppointments()); setError(""); }
    catch { setError("Không thể tải lịch hẹn được phân công."); }
    finally { setLoading(false); }
  };

  useEffect(() => {
    if (!user || !["Staff", "Admin"].includes(user.role)) { navigate("/login", { replace: true }); return; }
    void load();
  }, [navigate, user]);

  return <main className="min-h-[100dvh] bg-[#f6faff] text-[#10213f]">
    <header className="flex min-h-20 items-center justify-between border-b border-[#dce7f1] bg-white px-6 lg:px-[6%]">
      <strong className="text-2xl text-[#0768dd]">Nailify<span className="text-[#50b8ed]">.</span></strong>
      <div className="flex items-center gap-4"><span className="text-sm font-bold">{user?.fullName}</span><button className="rounded-lg border border-[#dce7f1] px-3 py-2 text-sm" onClick={logout}>Đăng xuất</button></div>
    </header>
    <div className="mx-auto max-w-6xl px-5 py-10">
      <p className="text-xs font-bold uppercase tracking-[.08em] text-[#0768dd]">Vận hành tiệm</p>
      <h1 className="mt-3 text-4xl font-extrabold tracking-[-.04em]">Lịch hẹn được phân công</h1>
      <p className="mt-3 text-[#61708c]">Lịch mới được tự động xác nhận sau khi payOS báo đã nhận tiền cọc. Nhân viên chỉ xác nhận lại lịch do khách đổi giờ.</p>
      {loading ? <p className="mt-8">Đang tải lịch…</p> : error ? <p className="mt-8 rounded-xl bg-red-50 p-4 text-red-700">{error}</p> : <section className="mt-8 grid gap-4">
        {items.map((item) => {
          const action = nextAction[item.status];
          return <article key={item.id} className="grid gap-5 rounded-2xl border border-[#dce7f1] bg-white p-5 md:grid-cols-[1fr_auto] md:items-center">
            <div>
              <div className="flex flex-wrap items-center gap-3">
                <strong className="text-lg">{item.serviceName}</strong>
                <span className={`rounded-full px-3 py-1 text-xs font-bold ${appointmentStatusClasses[item.status] ?? "bg-slate-100 text-slate-700"}`}>{appointmentStatusLabels[item.status] ?? item.status}</span>
                {item.deposit && <span className="text-xs text-[#61708c]">Cọc: {depositStatusLabels[item.deposit.status] ?? item.deposit.status} ({formatMoney(item.deposit.amount)})</span>}
              </div>
              <p className="mt-2 text-sm text-[#61708c]">{new Date(`${item.appointmentDate}T00:00:00`).toLocaleDateString("vi-VN")} · {item.startTime.slice(0, 5)}–{item.endTime.slice(0, 5)} · {item.staffName}</p>
              <p className="mt-1 text-xs text-[#8291a5]">{item.appointmentCode}</p>
            </div>
            {action && <button type="button" className="rounded-xl bg-[#0768dd] px-5 py-3 text-sm font-bold text-white hover:bg-[#075fc3]" onClick={async () => { await updateAppointmentStatus(item.id, action.key); await load(); }}>{action.label}</button>}
          </article>;
        })}
        {items.length === 0 && <p className="rounded-2xl border border-dashed border-[#bed2e8] bg-white p-10 text-center text-[#61708c]">Chưa có lịch hẹn nào.</p>}
      </section>}
    </div>
  </main>;
}
