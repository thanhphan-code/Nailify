import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { AxiosError } from "axios";
import {
  createAppointment,
  getAvailableSlots,
  getBookingStaff,
  getCompatibleDesigns,
  type BookingSlot,
  type BookingStaff,
  type CompatibleDesign,
} from "@/features/bookings/api";
import { useAuth } from "@/hooks/useAuth";
import { useServiceSelectionStore } from "@/store/serviceSelectionStore";
import heroImage from "@/assets/images/nailify-hero.png";

const toDateInputValue = (date: Date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};
const today = toDateInputValue(new Date());
const maxDate = toDateInputValue(new Date(Date.now() + 30 * 86400000));
const getInitialBookingDate = () => {
  const date = new Date();
  while (date.getDay() === 0) date.setDate(date.getDate() + 1);
  return toDateInputValue(date);
};
const formatTime = (time: string) => time.slice(0, 5);
const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value);

export default function BookingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const service = useServiceSelectionStore((state) => state.items[0]);
  const clear = useServiceSelectionStore((state) => state.clear);
  const [designs, setDesigns] = useState<CompatibleDesign[]>([]);
  const [staff, setStaff] = useState<BookingStaff[]>([]);
  const [designId, setDesignId] = useState("");
  const [staffId, setStaffId] = useState("");
  const [date, setDate] = useState(getInitialBookingDate);
  const [slots, setSlots] = useState<BookingSlot[]>([]);
  const [startTime, setStartTime] = useState("");
  const [totalDurationMinutes, setTotalDurationMinutes] = useState(service?.durationMinutes ?? 0);
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);
  const [optionsLoading, setOptionsLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const chosenDesign = useMemo(
    () => designs.find((item) => item.nailDesignId === designId),
    [designId, designs],
  );
  const chosenStaff = useMemo(
    () => staff.find((item) => item.id === staffId),
    [staff, staffId],
  );

  useEffect(() => {
    if (!isAuthenticated)
      navigate("/login", { state: { returnUrl: "/booking" }, replace: true });
    if (!service) navigate("/services", { replace: true });
  }, [isAuthenticated, navigate, service]);

  useEffect(() => {
    if (!service) return;
    setOptionsLoading(true);
    void Promise.all([
      getCompatibleDesigns(service.id),
      getBookingStaff(service.id),
    ])
      .then(([compatible, availableStaff]) => {
        setDesigns(compatible);
        setStaff(availableStaff);
        setDesignId(compatible[0]?.nailDesignId ?? "");
      })
      .catch(() =>
        setError("Không thể tải các lựa chọn đặt lịch. Vui lòng thử lại."),
      )
      .finally(() => setOptionsLoading(false));
  }, [service]);

  useEffect(() => {
    if (!service || !designId || !date) {
      setSlots([]);
      return;
    }
    setLoading(true);
    setError("");
    setStartTime("");
    void getAvailableSlots(service.id, designId, date, staffId || undefined)
      .then((result) => {
        setSlots(result.availableSlots);
        setTotalDurationMinutes(result.durationMinutes);
      })
      .catch(() => setError("Không thể tải giờ trống cho ngày này."))
      .finally(() => setLoading(false));
  }, [service, designId, date, staffId]);

  const submit = async () => {
    if (!service || !designId || !date || !startTime) {
      setError("Vui lòng chọn mẫu nail, ngày và giờ trống trước.");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      const booking = await createAppointment({
        serviceId: service.id,
        nailDesignId: designId,
        staffId: staffId || undefined,
        appointmentDate: date,
        startTime,
        note: note.trim() || undefined,
      });
      clear();
      navigate(`/deposit/${booking.id}`, { replace: true });
    } catch (exception) {
      const axiosError = exception as AxiosError<{ code?: string }>;
      const status = axiosError.response?.status;
      setError(
        status === 409 &&
          axiosError.response?.data?.code === "CUSTOMER_SLOT_CONFLICT"
          ? "Bạn đã có lịch hẹn vào thời gian này. Vui lòng chọn khung giờ khác."
          : status === 409
            ? "Khung giờ này vừa được đặt. Vui lòng chọn giờ trống khác."
            : "Không thể xác nhận lịch hẹn. Vui lòng thử lại.",
      );
    } finally {
      setSubmitting(false);
    }
  };

  if (!service) return null;
  return (
    <main className="min-h-screen bg-[#f6faff] px-5 py-8 text-[#10213f] sm:px-8">
      <div className="mx-auto max-w-4xl">
        <Link to="/services" className="text-sm font-bold text-[#0768dd]">
          ← Quay lại dịch vụ
        </Link>
        <header className="mt-7">
          <p className="text-xs font-bold uppercase tracking-[.08em] text-[#0768dd]">
            Đặt lịch làm nail
          </p>
          <h1 className="mt-3 text-4xl font-extrabold tracking-[-.04em]">
            Hoàn tất lịch hẹn của bạn.
          </h1>
          <p className="mt-3 text-[#61708c]">
            Chọn mẫu nail phù hợp, nhân viên và khung giờ thực sự còn trống.
          </p>
        </header>
        <div className="mt-7 grid gap-6 lg:grid-cols-[1.65fr_1fr]">
          <section className="rounded-2xl border border-[#dce7f1] bg-white p-5 sm:p-7">
            <label className="block text-sm font-bold">
              Mẫu nail
              <select
                value={designId}
                disabled={optionsLoading || designs.length === 0}
                onChange={(event) => setDesignId(event.target.value)}
                className="mt-2 w-full rounded-xl border border-[#c8d9e9] bg-white p-3"
              >
                {optionsLoading ? (
                  <option>Đang tải mẫu nail…</option>
                ) : designs.length ? (
                  designs.map((design) => (
                    <option
                      key={design.nailDesignId}
                      value={design.nailDesignId}
                    >
                      {design.name} (+{formatMoney(design.extraPrice)})
                    </option>
                  ))
                ) : (
                  <option>Không có mẫu nail phù hợp</option>
                )}
              </select>
            </label>
            <label className="mt-5 block text-sm font-bold">
              Nhân viên
              <select
                value={staffId}
                onChange={(event) => setStaffId(event.target.value)}
                className="mt-2 w-full rounded-xl border border-[#c8d9e9] bg-white p-3"
              >
                <option value="">Nhân viên bất kỳ đang rảnh (đề xuất)</option>
                {staff.map((person) => (
                  <option key={person.id} value={person.id}>
                    {person.fullName}
                    {person.specialty ? ` — ${person.specialty}` : ""}
                  </option>
                ))}
              </select>
            </label>
            <label className="mt-5 block text-sm font-bold">
              Ngày hẹn
              <input
                type="date"
                value={date}
                min={today}
                max={maxDate}
                onChange={(event) => setDate(event.target.value)}
                className="mt-2 w-full rounded-xl border border-[#c8d9e9] bg-white p-3"
              />
            </label>
            <div className="mt-5">
              <p className="text-sm font-bold">Khung giờ còn trống</p>
              {loading ? (
                <p className="mt-3 text-sm text-[#61708c]">
                  Đang kiểm tra giờ trống…
                </p>
              ) : slots.length ? (
                <div className="mt-3 grid grid-cols-3 gap-2 sm:grid-cols-4">
                  {slots.map((slot) => (
                    <button
                      key={slot.startTime}
                      type="button"
                      onClick={() => setStartTime(slot.startTime)}
                      className={
                        "rounded-lg border px-3 py-2 text-sm font-bold " +
                        (startTime === slot.startTime
                          ? "border-[#0768dd] bg-[#0768dd] text-white"
                          : "border-[#c8d9e9] text-[#254565]")
                      }
                    >
                      {formatTime(slot.startTime)}
                    </button>
                  ))}
                </div>
              ) : (
                <p className="mt-3 text-sm text-[#61708c]">
                  Không còn giờ trống trong ngày này.
                </p>
              )}
            </div>
            <label className="mt-5 block text-sm font-bold">
              Ghi chú{" "}
              <span className="font-normal text-[#61708c]">(không bắt buộc)</span>
              <textarea
                value={note}
                maxLength={500}
                onChange={(event) => setNote(event.target.value)}
                className="mt-2 min-h-24 w-full rounded-xl border border-[#c8d9e9] p-3"
              />
            </label>
            {error && (
              <p
                className="mt-4 rounded-xl bg-[#fff1f1] p-3 text-sm text-[#b42318]"
                role="alert"
              >
                {error}
              </p>
            )}
            <button
              type="button"
              disabled={submitting || !chosenDesign || !startTime}
              onClick={() => void submit()}
              className="mt-5 w-full rounded-xl bg-[#0768dd] px-4 py-3 font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
            >
              {submitting ? "Đang tạo lịch…" : "Tiếp tục đặt cọc"}
            </button>
          </section>
          <aside className="h-fit overflow-hidden rounded-2xl bg-[#10213f] text-white lg:sticky lg:top-6">
            <img
              src={service.imageUrl || heroImage}
              onError={(event) => { event.currentTarget.src = heroImage; }}
              alt={`Dịch vụ ${service.name}`}
              className="h-40 w-full object-cover opacity-90"
            />
            <div className="p-5">
            <p className="text-xs font-bold uppercase tracking-[.08em] text-[#8fc5ff]">
              Dịch vụ đã chọn
            </p>
            <div className="mt-3 flex items-start justify-between gap-4">
              <h2 className="text-xl font-extrabold">{service.name}</h2>
              <Link to="/services" className="shrink-0 text-xs font-bold text-[#8fc5ff] hover:text-white">Thay đổi</Link>
            </div>
            <p className="mt-2 text-sm leading-6 text-[#c9dbef]">
              {service.description || "Dịch vụ chăm sóc móng chuyên nghiệp tại Nailify."}
            </p>
            <dl className="mt-5 space-y-3 border-t border-[#36516e] pt-4 text-sm">
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Giá dịch vụ</dt><dd className="font-bold">{formatMoney(service.price)}</dd></div>
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Mẫu nail</dt><dd className="text-right font-bold">{chosenDesign?.name ?? "Đang chọn"}</dd></div>
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Phụ phí mẫu</dt><dd className="font-bold">+{formatMoney(chosenDesign?.extraPrice ?? 0)}</dd></div>
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Tổng thời lượng</dt><dd className="font-bold">{totalDurationMinutes} phút</dd></div>
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Nhân viên</dt><dd className="text-right font-bold">{chosenStaff?.fullName ?? "Tiệm tự phân công"}</dd></div>
              <div className="flex justify-between gap-4"><dt className="text-[#c9dbef]">Lịch hẹn</dt><dd className="text-right font-bold">{date ? new Date(`${date}T00:00:00`).toLocaleDateString("vi-VN") : "—"}{startTime ? ` · ${formatTime(startTime)}` : " · Chưa chọn giờ"}</dd></div>
            </dl>
            <div className="mt-5 border-t border-[#36516e] pt-4">
              <div className="flex items-end justify-between gap-4">
                <p className="text-sm text-[#c9dbef]">Tổng dự kiến</p>
                <strong className="text-3xl tabular-nums">{formatMoney(chosenDesign?.estimatedTotal ?? service.price)}</strong>
              </div>
              <p className="mt-3 rounded-lg bg-white/10 p-3 text-xs leading-5 text-[#d9e8f7]">
                Sau khi tạo lịch, bạn cần đặt cọc 30% qua payOS trong 10 phút để hệ thống tự động xác nhận và giữ khung giờ.
              </p>
            </div>
            </div>
          </aside>
        </div>
      </div>
    </main>
  );
}
