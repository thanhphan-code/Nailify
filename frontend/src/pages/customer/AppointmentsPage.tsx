import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  cancelAppointment,
  createServiceReview,
  getAvailableSlots,
  getMyAppointments,
  rescheduleAppointment,
  type BookingSlot,
  type CustomerAppointment,
} from "@/features/bookings/api";
import { useAuth } from "@/hooks/useAuth";
import CustomerHeader from "@/components/CustomerHeader";
import { appointmentStatusClasses, appointmentStatusLabels } from "@/features/bookings/status";

const formatDate = (value: string) =>
  new Intl.DateTimeFormat("vi-VN", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(`${value}T00:00:00`));

const formatTime = (value: string) => value.slice(0, 5);
const toDateInputValue = (date: Date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};
const minimumBookingDate = toDateInputValue(new Date());
const maximumBookingDate = toDateInputValue(
  new Date(Date.now() + 30 * 86400000),
);
const formatMoney = (value: number) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(value);


export default function AppointmentsPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [appointments, setAppointments] = useState<CustomerAppointment[]>([]);
  const [selected, setSelected] = useState<CustomerAppointment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [action, setAction] = useState<"cancel" | "reschedule" | "review" | null>(null);
  const [actionError, setActionError] = useState("");
  const [actionLoading, setActionLoading] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [rescheduleDate, setRescheduleDate] = useState("");
  const [rescheduleSlots, setRescheduleSlots] = useState<BookingSlot[]>([]);
  const [rescheduleStartTime, setRescheduleStartTime] = useState("");
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [reviewServiceId, setReviewServiceId] = useState("");

  useEffect(() => {
    if (!isAuthenticated) {
      navigate("/login", {
        state: { returnUrl: "/appointments" },
        replace: true,
      });
      return;
    }
    let cancelled = false;
    setLoading(true);
    getMyAppointments()
      .then((data) => {
        if (!cancelled) {
          setAppointments(data);
          setSelected(data[0] ?? null);
        }
      })
      .catch(
        () =>
          !cancelled &&
          setError("Không thể tải danh sách cuộc hẹn. Vui lòng thử lại."),
      )
      .finally(() => !cancelled && setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    setAction(null);
    setActionError("");
    setCancelReason("");
    setRescheduleDate(selected?.appointmentDate ?? "");
    setRescheduleSlots([]);
    setRescheduleStartTime("");
    setReviewRating(5);
    setReviewComment("");
    setReviewServiceId("");
  }, [selected?.id]);

  useEffect(() => {
    if (action !== "reschedule" || !selected?.nailDesignId || !rescheduleDate)
      return;
    setActionLoading(true);
    setActionError("");
    getAvailableSlots(selected.serviceId, selected.nailDesignId, rescheduleDate)
      .then((result) => setRescheduleSlots(result.availableSlots))
      .catch(() => setActionError("Không thể tải khung giờ cho ngày đã chọn."))
      .finally(() => setActionLoading(false));
  }, [action, rescheduleDate, selected]);

  const refreshAppointments = async (selectedId: string) => {
    const data = await getMyAppointments();
    setAppointments(data);
    setSelected(data.find((item) => item.id === selectedId) ?? data[0] ?? null);
  };

  const submitCancellation = async () => {
    if (!selected) return;
    setActionLoading(true);
    setActionError("");
    try {
      await cancelAppointment(selected.id, cancelReason);
      await refreshAppointments(selected.id);
      setAction(null);
    } catch {
      setActionError(
        "Không thể hủy cuộc hẹn này. Cuộc hẹn có thể đã bắt đầu hoặc đổi trạng thái.",
      );
    } finally {
      setActionLoading(false);
    }
  };

  const submitReschedule = async () => {
    if (!selected || !rescheduleDate || !rescheduleStartTime) return;
    setActionLoading(true);
    setActionError("");
    try {
      await rescheduleAppointment(selected.id, {
        appointmentDate: rescheduleDate,
        startTime: rescheduleStartTime,
      });
      await refreshAppointments(selected.id);
      setAction(null);
    } catch {
      setActionError(
        "Không thể đổi sang khung giờ này. Vui lòng chọn một khung giờ khác.",
      );
    } finally {
      setActionLoading(false);
    }
  };

  const selectedCanBeChanged =
    selected &&
    ["Pending", "Confirmed"].includes(selected.status) &&
    new Date(`${selected.appointmentDate}T${selected.startTime}`) > new Date();
  const reviewableService = selected?.services?.find((service) => service.serviceId === reviewServiceId && !service.review);

  const submitReview = async () => {
    if (!selected || !reviewableService) return;
    setActionLoading(true);
    setActionError("");
    try {
      await createServiceReview(selected.id, reviewableService.serviceId, reviewRating, reviewComment);
      await refreshAppointments(selected.id);
      setAction(null);
    } catch {
      setActionError("Không thể gửi đánh giá. Dịch vụ phải hoàn thành và chưa được đánh giá trước đó.");
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <main className="home-page home-refresh">
      <CustomerHeader active="appointments" />

      <div className="home-content">
        <p className="text-xs font-bold uppercase tracking-[.08em] text-[#0768dd]">
          Lịch hẹn của tôi
        </p>
        <h1 className="mt-3 text-4xl font-extrabold tracking-[-.04em]">
          Các cuộc hẹn của bạn
        </h1>
        <p className="mt-3 text-[#61708c]">
          Xem lịch đã đặt, nhân viên phụ trách, dịch vụ và chi phí chi tiết.
        </p>

        {loading ? (
          <p className="mt-10 text-[#61708c]">Đang tải cuộc hẹn…</p>
        ) : error ? (
          <p
            className="mt-10 rounded-xl bg-red-50 p-4 text-red-700"
            role="alert"
          >
            {error}
          </p>
        ) : appointments.length === 0 ? (
          <section className="mt-10 rounded-2xl border border-dashed border-[#bed2e8] bg-white p-10 text-center">
            <h2 className="text-xl font-extrabold">Bạn chưa có cuộc hẹn nào</h2>
            <p className="mt-2 text-[#61708c]">
              Chọn một dịch vụ và khung giờ phù hợp để bắt đầu.
            </p>
            <Link
              to="/services"
              className="mt-6 inline-block rounded-xl bg-[#0768dd] px-5 py-3 font-bold text-white"
            >
              Đặt lịch ngay
            </Link>
          </section>
        ) : (
          <div className="mt-8 grid gap-6 lg:grid-cols-[1fr_1.2fr]">
            <section className="space-y-3" aria-label="Danh sách cuộc hẹn">
              {appointments.map((appointment) => (
                <button
                  key={appointment.id}
                  type="button"
                  onClick={() => setSelected(appointment)}
                  className={`w-full rounded-2xl border bg-white p-5 text-left transition ${selected?.id === appointment.id ? "border-[#0768dd] ring-2 ring-[#d7ebff]" : "border-[#dce7f1] hover:border-[#9fc8ef]"}`}
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs font-bold text-[#61708c]">
                        {appointment.appointmentCode}
                      </p>
                      <h2 className="mt-2 text-lg font-extrabold">
                        {appointment.serviceName}
                      </h2>
                    </div>
                    <span
                      className={`rounded-full px-3 py-1 text-xs font-bold ${appointmentStatusClasses[appointment.status] ?? "bg-slate-100 text-slate-700"}`}
                    >
                      {appointmentStatusLabels[appointment.status] ?? appointment.status}
                    </span>
                  </div>
                  <p className="mt-3 text-sm font-semibold">
                    {formatDate(appointment.appointmentDate)}
                  </p>
                  <p className="mt-1 text-sm text-[#61708c]">
                    {formatTime(appointment.startTime)} –{" "}
                    {formatTime(appointment.endTime)} ·{" "}
                    {appointment.staffName ?? "Đang phân công"}
                  </p>
                </button>
              ))}
            </section>

            {selected && (
              <aside className="h-fit rounded-2xl border border-[#dce7f1] bg-white p-6 lg:sticky lg:top-6">
                <div className="flex items-start justify-between gap-4 border-b border-[#e5edf5] pb-5">
                  <div>
                    <p className="text-xs font-bold uppercase tracking-[.08em] text-[#0768dd]">
                      Chi tiết cuộc hẹn
                    </p>
                    <h2 className="mt-2 text-2xl font-extrabold">
                      {selected.appointmentCode}
                    </h2>
                  </div>
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-bold ${appointmentStatusClasses[selected.status] ?? "bg-slate-100 text-slate-700"}`}
                  >
                    {appointmentStatusLabels[selected.status] ?? selected.status}
                  </span>
                </div>
                <dl className="mt-5 grid gap-5 sm:grid-cols-2">
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Ngày
                    </dt>
                    <dd className="mt-1 font-bold">
                      {formatDate(selected.appointmentDate)}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Thời gian
                    </dt>
                    <dd className="mt-1 font-bold">
                      {formatTime(selected.startTime)} –{" "}
                      {formatTime(selected.endTime)}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Dịch vụ
                    </dt>
                    <dd className="mt-1 font-bold">{selected.serviceName}</dd>
                  </div>
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Mẫu nail
                    </dt>
                    <dd className="mt-1 font-bold">
                      {selected.nailDesignName ?? "Không có"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Nhân viên
                    </dt>
                    <dd className="mt-1 font-bold">
                      {selected.staffName ?? "Đang phân công"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs font-bold uppercase text-[#7a899f]">
                      Ngày đặt
                    </dt>
                    <dd className="mt-1 font-bold">
                      {new Date(selected.createdAt).toLocaleDateString("vi-VN")}
                    </dd>
                  </div>
                </dl>
                <div className="mt-6 rounded-xl bg-[#f4f8fc] p-4">
                  <div className="flex justify-between text-sm">
                    <span>Giá dịch vụ</span>
                    <strong>{formatMoney(selected.servicePrice)}</strong>
                  </div>
                  <div className="mt-2 flex justify-between text-sm">
                    <span>Phụ phí mẫu nail</span>
                    <strong>{formatMoney(selected.designExtraPrice)}</strong>
                  </div>
                  <div className="mt-4 flex justify-between border-t border-[#d8e3ed] pt-4 text-lg">
                    <span className="font-bold">Tổng cộng</span>
                    <strong>{formatMoney(selected.totalPrice)}</strong>
                  </div>
                </div>
                {selected.status === "AwaitingDeposit" && (
                  <section className="mt-5 rounded-xl border border-amber-200 bg-amber-50 p-4">
                    <h3 className="font-bold text-amber-900">Cần hoàn tất tiền cọc để giữ lịch</h3>
                    <p className="mt-1 text-sm text-amber-800">Hệ thống giữ khung giờ tối đa 10 phút và tự xác nhận ngay khi payOS báo thanh toán thành công.</p>
                    <button type="button" onClick={() => navigate(`/deposit/${selected.id}`)} className="mt-3 rounded-lg bg-[#0768dd] px-4 py-2.5 text-sm font-bold text-white">Thanh toán tiền cọc</button>
                  </section>
                )}
                {selected.status === "Completed" && (
                  <section className="mt-5 border-t border-[#e5edf5] pt-5">
                    <h3 className="font-extrabold">Đánh giá dịch vụ</h3>
                    {(selected.services ?? []).map((service) => (
                      <div className="mt-3 rounded-xl bg-[#f7fbff] p-4" key={service.serviceId}>
                        <div className="flex items-center justify-between gap-3">
                          <strong>{service.serviceName}</strong>
                          {service.review ? <span className="text-sm font-bold text-amber-700">{service.review.rating}/5 sao</span> : <button type="button" className="rounded-lg bg-[#0768dd] px-3 py-2 text-sm font-bold text-white" onClick={() => { setReviewServiceId(service.serviceId); setAction("review"); }}>Viết đánh giá</button>}
                        </div>
                        {service.review && <><p className="mt-2 text-sm text-[#61708c]">{service.review.comment || "Không có nhận xét."}</p><small className="mt-2 block text-[#7a899f]">{service.review.isApproved ? "Đã hiển thị công khai" : "Đang chờ tiệm duyệt"}</small></>}
                      </div>
                    ))}
                  </section>
                )}
                {action === "review" && reviewableService && (
                  <section className="mt-5 rounded-xl border border-[#bed8ef] bg-[#f4f8fc] p-4">
                    <h3 className="font-bold">Đánh giá {reviewableService.serviceName}</h3>
                    <fieldset className="mt-3">
                      <legend className="text-sm font-semibold">Mức độ hài lòng</legend>
                      <div className="mt-2 flex gap-2">
                        {[1, 2, 3, 4, 5].map((rating) => <button key={rating} type="button" aria-label={`${rating} sao`} aria-pressed={reviewRating === rating} onClick={() => setReviewRating(rating)} className={`h-10 w-10 rounded-lg border text-lg ${reviewRating >= rating ? "border-amber-400 bg-amber-50 text-amber-500" : "border-slate-200 bg-white text-slate-300"}`}>★</button>)}
                      </div>
                    </fieldset>
                    <label className="mt-4 block text-sm font-semibold">Nhận xét
                      <textarea value={reviewComment} maxLength={1000} onChange={(event) => setReviewComment(event.target.value)} className="mt-2 min-h-24 w-full rounded-lg border border-[#c8d9e9] bg-white p-3 font-normal" placeholder="Chia sẻ trải nghiệm thực tế của bạn" />
                    </label>
                    <p className="mt-2 text-xs text-[#61708c]">Đánh giá sẽ được hiển thị trên trang chủ sau khi tiệm duyệt.</p>
                    <div className="mt-4 flex gap-2">
                      <button type="button" disabled={actionLoading} onClick={() => void submitReview()} className="rounded-lg bg-[#0768dd] px-4 py-2 text-sm font-bold text-white disabled:opacity-50">{actionLoading ? "Đang gửi…" : "Gửi đánh giá"}</button>
                      <button type="button" disabled={actionLoading} onClick={() => setAction(null)} className="rounded-lg px-4 py-2 text-sm font-bold text-[#47627d]">Quay lại</button>
                    </div>
                  </section>
                )}
                {selectedCanBeChanged && (
                  <div className="mt-5 flex flex-wrap gap-3 border-t border-[#e5edf5] pt-5">
                    <button
                      type="button"
                      onClick={() => setAction("reschedule")}
                      className="rounded-xl bg-[#0768dd] px-4 py-2.5 text-sm font-bold text-white hover:bg-[#075fc3]"
                    >
                      Đổi lịch
                    </button>
                    <button
                      type="button"
                      onClick={() => setAction("cancel")}
                      className="rounded-xl border border-red-200 px-4 py-2.5 text-sm font-bold text-red-700 hover:bg-red-50"
                    >
                      Hủy cuộc hẹn
                    </button>
                  </div>
                )}
                {action === "cancel" && (
                  <section className="mt-5 rounded-xl border border-red-200 bg-red-50 p-4">
                    <h3 className="font-bold text-red-900">
                      Xác nhận hủy cuộc hẹn
                    </h3>
                    <label className="mt-3 block text-sm text-red-800">
                      Lý do (không bắt buộc)
                      <textarea
                        value={cancelReason}
                        maxLength={500}
                        onChange={(event) =>
                          setCancelReason(event.target.value)
                        }
                        className="mt-2 min-h-20 w-full rounded-lg border border-red-200 bg-white p-3 text-[#10213f]"
                      />
                    </label>
                    <div className="mt-3 flex gap-2">
                      <button
                        type="button"
                        disabled={actionLoading}
                        onClick={() => void submitCancellation()}
                        className="rounded-lg bg-red-700 px-4 py-2 text-sm font-bold text-white disabled:opacity-50"
                      >
                        {actionLoading ? "Đang hủy…" : "Xác nhận hủy"}
                      </button>
                      <button
                        type="button"
                        disabled={actionLoading}
                        onClick={() => setAction(null)}
                        className="rounded-lg px-4 py-2 text-sm font-bold text-red-800"
                      >
                        Quay lại
                      </button>
                    </div>
                  </section>
                )}
                {action === "reschedule" && (
                  <section className="mt-5 rounded-xl border border-[#bed8ef] bg-[#f4f8fc] p-4">
                    <h3 className="font-bold">Chọn lịch mới</h3>
                    <label className="mt-3 block text-sm font-semibold">
                      Ngày
                      <input
                        type="date"
                        value={rescheduleDate}
                        min={minimumBookingDate}
                        max={maximumBookingDate}
                        onChange={(event) => {
                          setRescheduleDate(event.target.value);
                          setRescheduleStartTime("");
                        }}
                        className="mt-2 w-full rounded-lg border border-[#c8d9e9] bg-white p-3"
                      />
                    </label>
                    <p className="mt-4 text-sm font-semibold">
                      Khung giờ còn trống
                    </p>
                    {actionLoading ? (
                      <p className="mt-2 text-sm text-[#61708c]">
                        Đang kiểm tra…
                      </p>
                    ) : rescheduleSlots.length ? (
                      <div className="mt-2 grid grid-cols-4 gap-2">
                        {rescheduleSlots.map((slot) => (
                          <button
                            key={slot.startTime}
                            type="button"
                            onClick={() =>
                              setRescheduleStartTime(slot.startTime)
                            }
                            className={`rounded-lg border px-2 py-2 text-sm font-bold ${rescheduleStartTime === slot.startTime ? "border-[#0768dd] bg-[#0768dd] text-white" : "border-[#c8d9e9] bg-white"}`}
                          >
                            {formatTime(slot.startTime)}
                          </button>
                        ))}
                      </div>
                    ) : (
                      <p className="mt-2 text-sm text-[#61708c]">
                        Không còn khung giờ phù hợp.
                      </p>
                    )}
                    <div className="mt-4 flex gap-2">
                      <button
                        type="button"
                        disabled={actionLoading || !rescheduleStartTime}
                        onClick={() => void submitReschedule()}
                        className="rounded-lg bg-[#0768dd] px-4 py-2 text-sm font-bold text-white disabled:opacity-50"
                      >
                        Xác nhận đổi lịch
                      </button>
                      <button
                        type="button"
                        disabled={actionLoading}
                        onClick={() => setAction(null)}
                        className="rounded-lg px-4 py-2 text-sm font-bold text-[#47627d]"
                      >
                        Quay lại
                      </button>
                    </div>
                  </section>
                )}
                {actionError && (
                  <p
                    className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700"
                    role="alert"
                  >
                    {actionError}
                  </p>
                )}
                {selected.note && (
                  <div className="mt-5">
                    <h3 className="text-sm font-bold">Ghi chú</h3>
                    <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-[#61708c]">
                      {selected.note}
                    </p>
                  </div>
                )}
                {selected.cancellationReason && (
                  <div className="mt-5 rounded-xl bg-red-50 p-4">
                    <h3 className="text-sm font-bold text-red-800">
                      Lý do hủy
                    </h3>
                    <p className="mt-1 text-sm text-red-700">
                      {selected.cancellationReason}
                    </p>
                  </div>
                )}
              </aside>
            )}
          </div>
        )}
      </div>
    </main>
  );
}
