import { Link, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import heroImage from "@/assets/images/nailify-hero.png";
import { getHome, type HomeData } from "@/features/home/api";
import { useServiceSelectionStore } from "@/store/serviceSelectionStore";
import type { Service } from "@/types/service";
import { appointmentStatusLabels } from "@/features/bookings/status";
import CustomerHeader from "@/components/CustomerHeader";
import {
  getMyAppointments,
  type CustomerAppointment,
} from "@/features/bookings/api";
const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value);

export default function HomePage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [category, setCategory] = useState("All");
  const [homeData, setHomeData] = useState<HomeData | null>(null);
  const [homeError, setHomeError] = useState(false);
  const [search, setSearch] = useState("");
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [upcomingAppointment, setUpcomingAppointment] =
    useState<CustomerAppointment | null>(null);
  const designs = useMemo(
    () =>
      (homeData?.featuredDesigns ?? []).filter(
        (design) =>
          (category === "All" || design.categoryName === category) &&
          design.name.toLowerCase().includes(search.trim().toLowerCase()),
      ),
    [category, homeData, search],
  );
  const selectService = useServiceSelectionStore((state) => state.select);
  useEffect(() => {
    let cancelled = false;
    getHome().then((data) => {
      if (!cancelled) {
        setHomeData(data);
        setFavorites(new Set(data.featuredDesigns.filter((item) => item.isFavorite).map((item) => item.id)));
      }
    }).catch(() => !cancelled && setHomeError(true));
    return () => { cancelled = true; };
  }, []);
  // The old #booking UI used hard-coded data; redirect it to the API-backed flow.
  useEffect(() => {
    if (location.hash === "#booking") navigate("/services", { replace: true });
  }, [location.hash, navigate]);
  useEffect(() => {
    if (!isAuthenticated) {
      setUpcomingAppointment(null);
      return;
    }
    let cancelled = false;
    getMyAppointments()
      .then((items) => {
        if (cancelled) return;
        const now = new Date();
        const upcoming =
          items
            .filter(
              (item) =>
                ["AwaitingDeposit", "Pending", "Confirmed"].includes(item.status) &&
                new Date(`${item.appointmentDate}T${item.startTime}`) > now,
            )
            .sort(
              (a, b) =>
                new Date(`${a.appointmentDate}T${a.startTime}`).getTime() -
                new Date(`${b.appointmentDate}T${b.startTime}`).getTime(),
            )[0] ?? null;
        setUpcomingAppointment(upcoming);
      })
      .catch(() => {
        if (!cancelled) setUpcomingAppointment(null);
      });
    return () => {
      cancelled = true;
    };
  }, [isAuthenticated]);
  const beginBooking = () => navigate("/services");
  const toggleFavorite = (id: string) =>
    setFavorites((current) => {
      const next = new Set(current);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  return (
    <main className="home-page home-refresh">
      <CustomerHeader active="home" />
      <div className="home-content" id="home">
        {homeError && <p className="mb-5 rounded-xl bg-amber-50 p-4 text-sm text-amber-800" role="status">Dữ liệu mới nhất của tiệm đang tạm thời chưa tải được. Vui lòng thử lại sau.</p>}
        <section
          className="home-hero home-hero-refresh"
          aria-labelledby="home-heading"
        >
          <div className="home-hero-copy">
            <p className="home-eyebrow">Làm đẹp theo lịch của bạn</p>
            <h1 id="home-heading">
              {homeData?.banner.title ?? "Tìm mẫu nail tiếp theo và đặt lịch chỉ trong vài phút."}
            </h1>
            <p>
              {homeData?.banner.description ?? "Khám phá mẫu nail, chọn dịch vụ và đặt lịch trực tuyến."}
            </p>
            <div className="home-hero-actions">
              <button
                type="button"
                className="home-primary-button"
                onClick={beginBooking}
              >
                Đặt lịch ngay
              </button>
              <a className="home-secondary-button" href="#designs">
                Xem mẫu nail
              </a>
            </div>
          </div>
          <div className="home-hero-art">
            <img
              src={heroImage}
              alt="Soft French manicure in a modern nail salon"
            />
          </div>
        </section>
        <section
          className="home-booking-search"
          id="booking"
          aria-labelledby="booking-heading"
        >
          <div>
            <p className="home-eyebrow">Lên lịch ghé tiệm</p>
            <h2 id="booking-heading">Đặt lịch hẹn còn trống</h2>
          </div>
          <div className="home-booking-fields">
            <p className="text-sm leading-6 text-[#61708c]">
              Chọn dịch vụ chính trước. Bước tiếp theo sẽ hiển thị mẫu nail phù hợp,
              nhân viên có thể phục vụ và các khung giờ còn trống.
            </p>
            <button
              type="button"
              className="home-find-button"
              onClick={beginBooking}
            >
              Chọn dịch vụ
            </button>
          </div>
        </section>
        {isAuthenticated && upcomingAppointment && (
          <section
            className="home-upcoming"
            id="appointment"
            aria-labelledby="appointment-heading"
          >
            <div>
              <p className="home-eyebrow">Lịch hẹn sắp tới</p>
              <h2 id="appointment-heading">
                {upcomingAppointment.serviceName}{" "}
                <span>{upcomingAppointment.nailDesignName}</span>
              </h2>
              <p>
                {new Date(
                  `${upcomingAppointment.appointmentDate}T${upcomingAppointment.startTime}`,
                ).toLocaleString("vi-VN", {
                  weekday: "short",
                  day: "2-digit",
                  month: "2-digit",
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </p>
              <p>Nhân viên: {upcomingAppointment.staffName ?? "Đang phân công"}</p>
            </div>
            <div className="home-appointment-summary">
              <strong>{appointmentStatusLabels[upcomingAppointment.status] ?? upcomingAppointment.status}</strong>
              <span>
                Tổng dự kiến{" "}
                <b>{formatMoney(upcomingAppointment.totalPrice)}</b>
              </span>
              <div>
                {upcomingAppointment.status === "AwaitingDeposit" ? (
                  <button type="button" onClick={() => navigate(`/deposit/${upcomingAppointment.id}`)}>Thanh toán tiền cọc</button>
                ) : (
                  <>
                    <button type="button" onClick={() => navigate("/appointments")}>Xem chi tiết</button>
                    <button type="button" onClick={() => navigate("/appointments")}>Đổi lịch</button>
                    <button type="button" className="home-text-button" onClick={() => navigate("/appointments")}>Hủy lịch</button>
                  </>
                )}
              </div>
            </div>
          </section>
        )}
        <section
          className="home-designs"
          id="designs"
          aria-labelledby="designs-heading"
        >
          <div className="home-section-heading">
            <div>
              <h2 id="designs-heading">Mẫu nail nổi bật</h2>
              <p>
                Những mẫu nổi bật đáng lưu cho lần hẹn tiếp theo của bạn.
              </p>
            </div>
            <Link to="/nail-designs">Xem tất cả mẫu</Link>
          </div>
          <div className="home-discovery">
            <label className="home-search-field" htmlFor="nail-search">
              <input
                id="nail-search"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm mẫu nail"
              />
            </label>
            <div className="home-categories">
              <div>
                {["All", ...(homeData?.categories.map((item) => item.name) ?? [])].map((item) => (
                  <button
                    type="button"
                    key={item}
                    className={item === category ? "is-selected" : ""}
                    onClick={() => setCategory(item)}
                  >
                    {item === "All" ? "Tất cả" : item}
                  </button>
                ))}
              </div>
            </div>
          </div>
          {designs.length ? (
            <div className="home-design-grid">
              {designs.slice(0, 4).map((design) => (
                <article className="home-design-card" key={design.id}>
                  <img
                    src={design.imageUrl}
                    alt={design.name + " nail design"}
                    onError={(event) => {
                      event.currentTarget.src = heroImage;
                    }}
                  />
                  <div>
                    <button
                      className="home-favorite"
                      type="button"
                      aria-pressed={favorites.has(design.id)}
                      onClick={() => toggleFavorite(design.id)}
                    >
                      {favorites.has(design.id) ? "Đã lưu" : "Lưu"}
                    </button>
                    <h3>{design.name}</h3>
                    <p>{design.categoryName}</p>
                    <strong>{"Phụ phí mẫu: +" + formatMoney(design.additionalPrice)}</strong>
                    <small>Thêm khoảng {design.additionalDurationMinutes} phút</small>
                  </div>
                </article>
              ))}
            </div>
          ) : (
            <p className="home-empty-state">
              Không tìm thấy mẫu nail. Hãy thử từ khóa hoặc danh mục khác.
            </p>
          )}
        </section>
        <section
          className="home-services-refresh"
          id="services"
          aria-labelledby="services-heading"
        >
          <div className="home-section-heading">
            <div>
              <h2 id="services-heading">Dịch vụ được yêu thích</h2>
              <p>
                Chọn dịch vụ chính trước. Bạn có thể thêm mẫu nail trong bước đặt lịch.
              </p>
            </div>
            <Link to="/services">Xem tất cả dịch vụ</Link>
          </div>
          <div className="home-service-grid">
            {(homeData?.popularServices ?? []).map((item) => (
              <article className="home-service-card" key={item.id}>
                <span>✦</span>
                <h3>{item.name}</h3>
                <p>
                  {item.description ?? "Dịch vụ chăm sóc móng chuyên nghiệp với thời gian và giá rõ ràng."}
                </p>
                <strong>
                  {item.durationMinutes} phút <b>{"Từ " + formatMoney(item.basePrice)}</b>
                </strong>
                <button
                  type="button"
                  onClick={() => {
                    if (!isAuthenticated) { navigate("/login", { state: { returnUrl: "/services" } }); return; }
                    selectService({ id: item.id, name: item.name, category: "Other", price: item.basePrice, durationMinutes: item.durationMinutes, description: item.description, imageUrl: item.imageUrl, isActive: true } as Service);
                    navigate("/booking");
                  }}
                >
                  Đặt ngay
                </button>
              </article>
            ))}
          </div>
        </section>
        <section className="home-how-it-works" aria-labelledby="how-heading">
          <div>
            <p className="home-eyebrow">Đặt lịch thật đơn giản</p>
            <h2 id="how-heading">Từ cảm hứng đến lịch hẹn</h2>
          </div>
          <ol>
            <li>
              <b>Chọn mẫu nail</b>
              <span>Tìm phong cách bạn yêu thích.</span>
            </li>
            <li>
              <b>Chọn dịch vụ</b>
              <span>Chọn dịch vụ nền phù hợp.</span>
            </li>
            <li>
              <b>Chọn ngày và giờ</b>
              <span>Xem khung giờ phù hợp với bạn.</span>
            </li>
            <li>
              <b>Xác nhận lịch hẹn</b>
              <span>Kiểm tra và đặt lịch an tâm.</span>
            </li>
          </ol>
        </section>
        <section
          className="home-reviews"
          id="reviews"
          aria-labelledby="reviews-heading"
        >
          <div className="home-section-heading">
            <div>
              <h2 id="reviews-heading">Khách hàng yêu thích</h2>
              <p>Những đánh giá gần đây đã được duyệt.</p>
            </div>
            <a href="#reviews">Xem tất cả đánh giá</a>
          </div>
          <div>
            {(homeData?.featuredReviews ?? []).map((review) => (
              <article key={`${review.customerName}-${review.createdAt}`}>
                <span aria-label={`${review.rating} trên 5 sao`}>{review.rating} / 5</span>
                <p>{review.comment}</p>
                <strong>{review.customerName}</strong>
                <small>
                  {review.serviceName} - {new Date(review.createdAt).toLocaleDateString("vi-VN")}
                </small>
              </article>
            ))}
            {!homeData?.featuredReviews.length && <p className="home-empty-state">Chưa có đánh giá được duyệt.</p>}
          </div>
        </section>
        <section className="home-final-cta">
          <div>
            <h2>Sẵn sàng cho bộ móng mới?</h2>
            <p>Chọn mẫu nail và đặt lịch ngay hôm nay.</p>
          </div>
          <div>
            <button
              type="button"
              className="home-primary-button"
              onClick={beginBooking}
            >
              Đặt lịch ngay
            </button>
            <a className="home-secondary-button" href="#designs">
              Khám phá mẫu nail
            </a>
          </div>
        </section>
      </div>
      <footer className="home-footer">
        <div>
          <Link to="/" className="home-logo">
            Nailify<span>.</span>
          </Link>
          <p>Khám phá mẫu nail bạn yêu thích, rồi đặt khung giờ phù hợp.</p>
        </div>
        <div>
          <strong>Ghé thăm tiệm</strong>
          <p>
            {homeData?.salon.address || "18 Nguyễn Huệ, Quận 1, Thành phố Hồ Chí Minh"}
            <br />
            {homeData?.salon.phone || "(028) 3822 8899"}
          </p>
        </div>
        <div>
          <strong>Giờ mở cửa</strong>
          <p>
            {(homeData?.salon.openingHours.length ? homeData.salon.openingHours : ["Thứ Hai - Thứ Sáu: 09:00 - 20:00", "Thứ Bảy - Chủ Nhật: 09:00 - 18:00"]).map((hours) => <span className="block" key={hours}>{hours}</span>)}
          </p>
        </div>
        <div>
          <strong>Khám phá</strong>
          <Link to="/services">Dịch vụ</Link>
          <Link to="/nail-designs">Mẫu nail</Link>
          <a href="#booking">Chính sách đặt lịch</a>
          <a href="#reviews">Chính sách bảo mật</a>
        </div>
      </footer>
    </main>
  );
}
