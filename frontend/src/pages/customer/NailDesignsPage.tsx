import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import CustomerHeader from "@/components/CustomerHeader";
import heroImage from "@/assets/images/nailify-hero.png";
import { useAuth } from "@/hooks/useAuth";
import {
  getNailDesignCategories,
  getNailDesigns,
  type NailDesignCategory,
  type NailDesignListItem,
} from "@/features/nail-designs/api";

const formatMoney = (value: number) => new Intl.NumberFormat("vi-VN", {
  style: "currency", currency: "VND", maximumFractionDigits: 0,
}).format(value);
type SortOption = "recommended" | "price-asc" | "price-desc" | "newest";

export default function NailDesignsPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [params, setParams] = useSearchParams();
  const [items, setItems] = useState<NailDesignListItem[]>([]);
  const [categories, setCategories] = useState<NailDesignCategory[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const category = params.get("category") ?? "";
  const query = params.get("search") ?? "";
  const sort = (params.get("sort") as SortOption | null) ?? "recommended";
  const selected = items.find((item) => item.id === selectedId) ?? null;

  const update = (values: Record<string, string>) => {
    const next = new URLSearchParams(params);
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key));
    setParams(next);
  };

  useEffect(() => {
    getNailDesignCategories().then(setCategories).catch(() => setCategories([]));
  }, []);

  useEffect(() => {
    let cancelled = false;
    setLoading(true); setError("");
    getNailDesigns({ category: category || undefined, search: query || undefined, sort, pageSize: 50 })
      .then((response) => {
        if (cancelled) return;
        setItems(response.items);
        setTotal(response.pagination.totalItems);
        setFavorites(new Set(response.items.filter((item) => item.isFavorite).map((item) => item.id)));
      })
      .catch(() => !cancelled && setError("Không thể tải mẫu nail. Vui lòng thử lại."))
      .finally(() => !cancelled && setLoading(false));
    return () => { cancelled = true; };
  }, [category, query, sort]);

  const choose = () => navigate(isAuthenticated ? "/services" : "/login", { state: { returnUrl: "/services" } });
  const toggleFavorite = (id: string) => setFavorites((current) => {
    const next = new Set(current);
    next.has(id) ? next.delete(id) : next.add(id);
    return next;
  });

  return (
    <main className="design-page design-refresh">
      <CustomerHeader active="designs" />
      <div className="design-layout design-layout-refresh customer-page-content">
        <section className="design-main">
          <header className="design-page-intro">
            <p className="design-side-label">Mẫu nail</p>
            <h1>Chọn phong cách bạn yêu thích.</h1>
            <p>Khám phá mẫu nail, so sánh phụ phí và chọn dịch vụ phù hợp để đặt lịch.</p>
          </header>
          <section className="design-toolbar" aria-label="Bộ lọc mẫu nail">
            <label className="design-search design-search-refresh"><span>Tìm kiếm</span><input value={query} onChange={(event) => update({ search: event.target.value })} placeholder="Tìm mẫu nail" /></label>
            <label className="design-select-label">Danh mục<select value={category} onChange={(event) => update({ category: event.target.value })}><option value="">Tất cả</option>{categories.map((item) => <option key={item.id} value={item.name}>{item.name} ({item.designCount})</option>)}</select></label>
            <label className="design-select-label">Sắp xếp<select value={sort} onChange={(event) => update({ sort: event.target.value })}><option value="recommended">Đề xuất</option><option value="newest">Mới nhất</option><option value="price-asc">Phụ phí thấp đến cao</option><option value="price-desc">Phụ phí cao đến thấp</option></select></label>
          </section>
          <p className="design-result-summary">{loading ? "Đang tải mẫu nail…" : `Có ${total} mẫu nail`}</p>
          {error ? (
            <section className="design-empty"><h2>{error}</h2><button type="button" onClick={() => window.location.reload()}>Thử lại</button></section>
          ) : (
            <div className="design-card-grid design-card-grid-refresh">
              {items.map((design) => (
                <article className={`design-card ${selectedId === design.id ? "is-current" : ""}`} key={design.id}>
                  <button type="button" className="design-card-select" onClick={() => setSelectedId(design.id)}><img src={design.imageUrl} alt={`Mẫu ${design.name}`} onError={(event) => { event.currentTarget.src = heroImage; }} /></button>
                  <div className="design-card-info">
                    <button type="button" className="design-favorite-button" aria-pressed={favorites.has(design.id)} onClick={() => toggleFavorite(design.id)}>{favorites.has(design.id) ? "Đã lưu" : "Lưu"}</button>
                    <h2>{design.name}</h2><p>{design.category.name}</p>
                    <strong>Phụ phí: +{formatMoney(design.additionalPrice)}</strong><small>Thêm {design.additionalDurationMinutes} phút</small>
                  </div>
                  <button type="button" className="design-details-button" onClick={() => setSelectedId(design.id)}>{selectedId === design.id ? "Đã chọn" : "Chọn mẫu"}</button>
                </article>
              ))}
            </div>
          )}
        </section>
        <aside className="design-sidebar">
          <section className="design-selection-card">
            {selected ? <>
              <p className="design-side-label">Mẫu bạn chọn</p>
              <div className="design-selected-image"><img src={selected.imageUrl} alt={selected.name} onError={(event) => { event.currentTarget.src = heroImage; }} /></div>
              <div className="design-selected-title"><h2>{selected.name}</h2><strong>+{formatMoney(selected.additionalPrice)}</strong></div>
              <span className="design-category-tag">{selected.category.name}</span>
              <p className="design-description">Phụ phí mẫu: +{formatMoney(selected.additionalPrice)}<br />Thêm {selected.additionalDurationMinutes} phút</p>
              <button type="button" className="design-book-button" onClick={choose}>Chọn dịch vụ</button>
            </> : <div className="design-selection-empty"><p className="design-side-label">Mẫu bạn chọn</p><h2>Chưa chọn mẫu</h2><p>Chọn một mẫu nail để xem chi tiết và tiếp tục đặt lịch.</p></div>}
          </section>
        </aside>
      </div>
    </main>
  );
}
