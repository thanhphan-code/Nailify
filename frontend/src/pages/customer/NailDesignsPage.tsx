import { Link, useNavigate } from "react-router-dom";
import { useMemo, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { mockCategories, mockNailDesigns, type MockNailDesign } from "@/mocks/homeData";

const galleryDesigns: MockNailDesign[] = [...mockNailDesigns, { id: "design-baby-boomer", name: "Baby Boomer", category: "French", extraPrice: 10, imageUrl: "https://images.unsplash.com/photo-1610992015732-2449b76344bc?auto=format&fit=crop&w=900&q=85" }, { id: "design-milk-tea", name: "Milk Tea Minimal", category: "Minimal", extraPrice: 6, imageUrl: "https://images.unsplash.com/photo-1604902396830-aca29e19d4c9?auto=format&fit=crop&w=900&q=85" }];

function SearchIcon() { return <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="5.5" /><path d="m16 16 4 4" /></svg>; }
function CalendarIcon() { return <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3.5" y="5" width="17" height="15" rx="2" /><path d="M7.5 3v4M16.5 3v4M3.5 10h17" /></svg>; }

export default function NailDesignsPage() {
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();
  const [category, setCategory] = useState<(typeof mockCategories)[number]>("All");
  const [query, setQuery] = useState("");
  const [selectedId, setSelectedId] = useState(galleryDesigns[0].id);
  const [visibleCount, setVisibleCount] = useState(8);
  const filteredDesigns = useMemo(() => galleryDesigns.filter((design) => {
    const matchesCategory = category === "All" || design.category === category;
    const keyword = query.trim().toLowerCase();
    return matchesCategory && (!keyword || `${design.name} ${design.category}`.toLowerCase().includes(keyword));
  }), [category, query]);
  const selectedDesign = galleryDesigns.find((design) => design.id === selectedId) ?? filteredDesigns[0] ?? galleryDesigns[0];
  const displayName = user?.fullName?.trim() || "Customer";
  const bookDesign = () => navigate(isAuthenticated ? { pathname: "/", hash: "services" } : "/login", { state: { selectedNailDesignId: selectedDesign.id } });

  return <main className="design-page">
    <header className="design-header">
      <Link to="/" className="design-logo">Nailify<span>&bull;</span></Link>
      <nav className="design-nav" aria-label="Primary navigation"><Link to="/">Home</Link><Link className="is-active" to="/nail-designs">Nail Designs</Link><Link to="/services">Services</Link><a href="#appointments">My Appointments</a><a href="#reviews">Reviews</a></nav>
      <div className="design-profile">{isAuthenticated ? <><div className="design-avatar" aria-hidden="true">{displayName.charAt(0).toUpperCase()}</div><div className="design-profile-copy"><strong>{displayName}</strong><span>{user?.role ?? "Customer"}</span></div></> : <div className="design-auth-links"><Link to="/login">Log in</Link><Link to="/register">Sign up</Link></div>}</div>
    </header>
    <div className="design-layout">
      <section className="design-main" aria-labelledby="design-page-title">
        <section className="design-hero"><div className="design-hero-copy"><p>NAIL DESIGNS MADE FOR YOU</p><h1 id="design-page-title">Explore your next nail design.</h1><span>Browse trending styles, find your favorite, and book in minutes.</span><label className="design-search" htmlFor="design-search"><SearchIcon /><input id="design-search" value={query} onChange={(event) => { setQuery(event.target.value); setVisibleCount(8); }} placeholder="Search nail designs by name, style, or keyword..." /></label><div className="design-filter-list" aria-label="Filter by category">{mockCategories.map((item) => <button key={item} type="button" className={item === category ? "is-selected" : ""} onClick={() => { setCategory(item); setVisibleCount(8); }}>{item}</button>)}</div></div><div className="design-hero-visual" aria-hidden="true"><div /><img src="https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=1200&q=88" alt="" /></div></section>
        {filteredDesigns.length ? <div className="design-card-grid">{filteredDesigns.slice(0, visibleCount).map((design) => <article className={`design-card ${selectedDesign.id === design.id ? "is-current" : ""}`} key={design.id}><button type="button" className="design-card-select" onClick={() => setSelectedId(design.id)} aria-label={`Select ${design.name}`}><img src={design.imageUrl} alt={`${design.name} nail design`} /></button><div className="design-card-info"><h2>{design.name}</h2><strong>+${design.extraPrice}</strong><p>{design.category}</p></div><button type="button" className="design-details-button" onClick={() => setSelectedId(design.id)}>View details</button></article>)}</div> : <p className="design-empty">No nail designs found. Try another search or category.</p>}
        {visibleCount < filteredDesigns.length && <button type="button" className="design-load-more" onClick={() => setVisibleCount((count) => count + 4)}>Load more designs <span>⌄</span></button>}
      </section>
      <aside className="design-sidebar" aria-label="Selected nail design"><section className="design-selection-card"><p className="design-side-label">Your selection</p><div className="design-selected-image"><img src={selectedDesign.imageUrl} alt={selectedDesign.name} /></div><div className="design-selected-title"><h2>{selectedDesign.name}</h2><strong>+${selectedDesign.extraPrice}</strong></div><span className="design-category-tag">{selectedDesign.category}</span><p className="design-description">A polished look selected to complement your next salon appointment.</p><button type="button" className="design-book-button" onClick={bookDesign}>{isAuthenticated ? "Book this design" : "Sign in to book"}</button></section><section className="design-booking-tip"><div className="design-tip-icon"><CalendarIcon /></div><div><h2>Booking is quick &amp; easy</h2><p>Choose your style, pick a time, and we'll take care of the rest.</p><button type="button" onClick={bookDesign}>Choose a time</button></div></section><section className="design-help-card"><span>✧</span><div><h2>Not sure what to pick?</h2><p>Our nail artists can help you find the perfect look in your appointment.</p><a href="#services">Learn more <b>&rarr;</b></a></div></section></aside>
    </div>
  </main>;
}
