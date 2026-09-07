import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { cancelDeposit, getDeposit, type DepositPayment } from "@/features/bookings/api";

const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", {
  style: "currency", currency: "VND", maximumFractionDigits: 0,
}).format(value);

export default function DepositPage() {
  const { appointmentId } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [payment, setPayment] = useState<DepositPayment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [remaining, setRemaining] = useState(0);
  const cancellationHandled = useRef(false);

  const loadPayment = useCallback(async () => {
    if (!appointmentId) return;
    const result = await getDeposit(appointmentId);
    setPayment(result);
    setRemaining(Math.max(0, Math.ceil((new Date(result.expiresAt).getTime() - Date.now()) / 1000)));
    setError("");
  }, [appointmentId]);

  useEffect(() => {
    if (!appointmentId) { navigate("/appointments", { replace: true }); return; }
    loadPayment()
      .catch((requestError) => setError(requestError.response?.data?.title || "Không thể tạo phiên thanh toán."))
      .finally(() => setLoading(false));
  }, [appointmentId, loadPayment, navigate]);

  useEffect(() => {
    const cancelledByPayOs = searchParams.get("payment") === "cancelled" || searchParams.get("cancel") === "true";
    if (!appointmentId || !cancelledByPayOs || cancellationHandled.current) return;
    cancellationHandled.current = true;
    cancelDeposit(appointmentId)
      .then(() => navigate("/appointments", { replace: true, state: { message: "Đã hủy thanh toán và giải phóng khung giờ." } }))
      .catch(() => loadPayment().catch(() => setError("Không thể cập nhật trạng thái thanh toán.")));
  }, [appointmentId, loadPayment, navigate, searchParams]);

  useEffect(() => {
    if (!payment || ["Approved", "Expired", "Rejected"].includes(payment.status)) return;
    const poller = window.setInterval(() => {
      loadPayment().catch(() => undefined);
    }, 2500);
    return () => window.clearInterval(poller);
  }, [loadPayment, payment?.status]);

  useEffect(() => {
    if (!payment || payment.status === "Approved") return;
    const timer = window.setInterval(() => setRemaining((value) => Math.max(0, value - 1)), 1000);
    return () => window.clearInterval(timer);
  }, [payment]);

  const countdown = useMemo(() => {
    const minutes = String(Math.floor(remaining / 60)).padStart(2, "0");
    const seconds = String(remaining % 60).padStart(2, "0");
    return `${minutes}:${seconds}`;
  }, [remaining]);

  if (loading) return <main className="min-h-screen bg-[#f6faff] p-8">Đang tạo phiên thanh toán an toàn…</main>;
  if (!payment) return <main className="min-h-screen bg-[#f6faff] p-8"><p className="text-red-700">{error}</p><Link to="/appointments">Về lịch hẹn</Link></main>;

  const approved = payment.status === "Approved";
  const expired = remaining <= 0 || ["Expired", "Rejected"].includes(payment.status);
  return (
    <main className="min-h-screen bg-[#f6faff] px-5 py-8 text-[#10213f]">
      <div className="mx-auto max-w-xl">
        <Link to="/appointments" className="text-sm font-bold text-[#0768dd]">← Về lịch hẹn</Link>
        <section className="mt-6 overflow-hidden rounded-3xl border border-[#dce7f1] bg-white">
          <header className="bg-[#10213f] p-7 text-white">
            <p className="text-xs font-bold uppercase tracking-[.08em] text-[#8fc5ff]">Thanh toán tiền cọc qua payOS</p>
            <h1 className="mt-3 text-3xl font-extrabold">{approved ? "Thanh toán thành công" : `Giữ lịch trong ${countdown}`}</h1>
            <p className="mt-3 text-sm text-[#c9dbef]">Hệ thống tự động xác nhận lịch hẹn ngay khi nhận webhook hợp lệ từ payOS.</p>
          </header>
          <div className="p-7">
            <p className="text-sm text-[#61708c]">Số tiền cần cọc</p>
            <strong className="mt-1 block text-3xl text-[#0768dd]">{formatMoney(payment.amount)}</strong>
            {approved ? (
              <><p className="mt-6 rounded-xl bg-emerald-50 p-4 text-sm font-semibold text-emerald-800">Tiền cọc đã được xác nhận và lịch hẹn đã được giữ.</p><Link className="mt-4 block text-center font-bold text-[#0768dd]" to="/appointments">Xem lịch hẹn</Link></>
            ) : !expired && payment.checkoutUrl ? (
              <a className="mt-6 block rounded-xl bg-[#0768dd] px-5 py-4 text-center text-sm font-bold text-white hover:bg-[#075fc3]" href={payment.checkoutUrl}>Thanh toán an toàn với payOS</a>
            ) : (
              <p className="mt-6 rounded-xl bg-amber-50 p-4 text-sm text-amber-800">Phiên thanh toán đã kết thúc. Khung giờ sẽ được hệ thống giải phóng.</p>
            )}
            {error && <p className="mt-4 text-sm text-red-700" role="alert">{error}</p>}
            {!approved && !expired && <p className="mt-5 text-xs leading-5 text-[#71829a]">Trang này tự kiểm tra trạng thái mỗi vài giây. Bạn không cần gửi biên lai hoặc chờ nhân viên xác nhận.</p>}
          </div>
        </section>
      </div>
    </main>
  );
}
