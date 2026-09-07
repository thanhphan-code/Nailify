import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getServices } from "@/features/services/api";
import { useAuth } from "@/hooks/useAuth";
import { useServiceSelectionStore } from "@/store/serviceSelectionStore";
import type { Service } from "@/types/service";
import heroImage from "@/assets/images/nailify-hero.png";
import CustomerHeader from "@/components/CustomerHeader";
const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value);

const categories = [
  { value: "All", label: "Tất cả" }, { value: "Manicure", label: "Chăm sóc móng tay" },
  { value: "Pedicure", label: "Chăm sóc móng chân" }, { value: "NailArt", label: "Nghệ thuật nail" },
  { value: "AddOn", label: "Dịch vụ thêm" }, { value: "Other", label: "Khác" },
];

export default function ServicesPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [category, setCategory] = useState("All");
  const [search, setSearch] = useState("");
  const [items, setItems] = useState<Service[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [retryKey, setRetryKey] = useState(0);
  const { items: selected, select, clear } = useServiceSelectionStore();

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      setError(false);
      try {
        const response = await getServices({
          category: category === "All" ? undefined : category,
          search: search.trim() || undefined,
          pageSize: 50,
        });
        if (!cancelled) setItems(response.items);
      } catch {
        if (!cancelled) {
          setItems([]);
          setError(true);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [category, search, retryKey]);

  const selectService = (service: Service) => {
    if (!isAuthenticated) {
      navigate("/login", { state: { returnUrl: "/services" } });
      return;
    }
    select(service);
    navigate("/booking");
  };

  return (
    <main className="design-page services-page">
      <CustomerHeader active="services" />
      <div className="customer-page-content">
        <header className="max-w-2xl">
          <p className="text-xs font-bold uppercase tracking-[.08em] text-[#0768dd]">
            Dịch vụ
          </p>
          <h1 className="mt-3 text-4xl font-extrabold tracking-[-.04em] sm:text-5xl">
            Chọn dịch vụ chính của bạn.
          </h1>
          <p className="mt-3 text-base leading-7 text-[#61708c]">
            Chọn một dịch vụ chính, sau đó chọn mẫu nail phù hợp và giờ hẹn còn trống.
          </p>
        </header>
        <section className="services-filter-panel" aria-label="Bộ lọc dịch vụ">
          <label className="services-filter-search" htmlFor="service-search">
            Tìm kiếm dịch vụ
            <input
              id="service-search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Nhập tên dịch vụ"
            />
          </label>
          <div className="services-category-list" aria-label="Danh mục dịch vụ">
            {categories.map((item) => (
              <button
                type="button"
                className={
                  "services-category-button" +
                  (category === item.value ? " is-selected" : "")
                }
                onClick={() => setCategory(item.value)}
                key={item.value}
              >
                {item.label}
              </button>
            ))}
          </div>
        </section>
        <p className="mt-5 text-sm text-[#60718a]" aria-live="polite">
          {loading
            ? "Đang tải dịch vụ..."
            : error
              ? "Không thể tải dịch vụ."
              : "Có " + items.length + " dịch vụ"}
        </p>
        {loading ? (
          <div className="mt-6 grid gap-5 md:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }, (_, index) => (
              <div
                className="animate-pulse rounded-2xl border border-[#e1e9f1] bg-white p-5"
                key={index}
              >
                <div className="h-40 rounded-xl bg-slate-100" />
                <div className="mt-4 h-4 w-24 rounded bg-slate-100" />
                <div className="mt-3 h-5 w-2/3 rounded bg-slate-100" />
              </div>
            ))}
          </div>
        ) : error ? (
          <section className="services-error-state" role="alert">
            <div className="services-error-icon" aria-hidden="true">
              !
            </div>
            <div>
              <h2>Dịch vụ tạm thời chưa khả dụng.</h2>
              <p>
                Danh sách dịch vụ sẽ hiển thị ngay khi máy chủ hoạt động trở lại.
              </p>
            </div>
            <button type="button" onClick={() => setRetryKey((key) => key + 1)}>
              Thử lại
            </button>
          </section>
        ) : items.length === 0 ? (
          <section className="mt-6 rounded-2xl border border-dashed border-[#bed2e8] bg-white p-8 text-center">
            <h2 className="text-lg font-extrabold">Không tìm thấy dịch vụ</h2>
            <p className="mt-2 text-sm text-[#61708c]">
              Hãy thử từ khóa hoặc danh mục khác.
            </p>
            <button
              type="button"
              className="mt-5 rounded-xl border border-[#87bdf2] px-4 py-3 text-sm font-bold text-[#0768dd]"
              onClick={() => {
                setSearch("");
                setCategory("All");
              }}
            >
              Xóa bộ lọc
            </button>
          </section>
        ) : (
          <div className="mt-6 grid gap-5 md:grid-cols-2 lg:grid-cols-3">
            {items.map((service) => {
              const imageUrl = service.imageUrl || heroImage;
              return (
                <article
                  className="service-card border-[#e0e8f0]"
                  key={service.id}
                >
                  <img
                    className={
                      "service-card-image service-card-image-" +
                      service.category
                    }
                    src={imageUrl}
                    onError={(event) => {
                      event.currentTarget.src = heroImage;
                    }}
                    alt={service.name}
                  />
                  <span className="rounded-full bg-[#edf6ff] px-3 py-1 text-xs font-semibold text-[#39709e]">
                    {service.category}
                  </span>
                  <h2 className="mt-3 text-lg font-extrabold">
                    {service.name}
                  </h2>
                  <p className="mt-1 min-h-10 text-sm leading-5 text-[#63738a]">
                    {service.description || "Dịch vụ chăm sóc móng chuyên nghiệp."}
                  </p>
                  <p className="mt-4 font-semibold">
                    {service.durationMinutes} phút{" "}
                    <span className="ml-2 text-[#0871df]">
                      {"Từ " + formatMoney(service.price)}
                    </span>
                  </p>
                  <button
                    type="button"
                    onClick={() => selectService(service)}
                    className="mt-5 min-h-11 w-full rounded-xl bg-[#0768dd] px-4 py-3 text-sm font-bold text-white hover:bg-[#075fc3]"
                  >
                    {!isAuthenticated
                      ? "Đăng nhập để chọn"
                      : "Đặt dịch vụ này"}
                  </button>
                </article>
              );
            })}
          </div>
        )}
      </div>
      {isAuthenticated && selected.length > 0 && (
        <aside
          id="selected-services"
          className="fixed bottom-4 left-4 right-4 z-40 mx-auto flex max-w-md items-center justify-between gap-4 rounded-2xl bg-[#172a42] p-4 text-white shadow-xl"
        >
          <div>
            <strong>Đã chọn {selected[0].name}</strong>
            <p className="mt-1 text-xs text-[#c8d9ea]">
              {"Từ " + formatMoney(selected[0].price) +
                " · " +
                selected[0].durationMinutes +
                " phút"}
            </p>
          </div>
          <div className="flex gap-2">
            <button
              type="button"
              className="rounded-lg px-3 py-2 text-xs font-bold text-[#d1e1f1]"
              onClick={clear}
            >
              Bỏ chọn
            </button>
            <button
              type="button"
              className="rounded-lg bg-white px-3 py-2 text-xs font-bold text-[#172a42]"
              onClick={() => navigate("/booking")}
            >
              Tiếp tục
            </button>
          </div>
        </aside>
      )}
    </main>
  );
}
