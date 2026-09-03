import { Link } from "react-router-dom";
import { useMemo, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { mockCategories, mockNailDesigns, mockServices, mockUpcomingAppointment } from "@/mocks/homeData";

export default function HomePage() {
  const { user, isAuthenticated, logout } = useAuth();
  const [selectedCategory, setSelectedCategory] = useState<(typeof mockCategories)[number]>("All");
  const [search, setSearch] = useState("");
  const filteredDesigns = useMemo(() => mockNailDesigns.filter((design) =>
    (selectedCategory === "All" || design.category === selectedCategory)
    && design.name.toLowerCase().includes(search.trim().toLowerCase()),
  ), [search, selectedCategory]);
  const displayName = user?.fullName?.trim() || "Customer";

  return (
    <main className="home-page">
      <header className="home-header">
        <Link to="/" className="home-logo">Nailify<span>&bull;</span></Link>
        <nav className="home-nav" aria-label="Primary navigation"><a className="is-active" href="#home">Home</a><Link to="/nail-designs">Nail Designs</Link><Link to="/services">Services</Link><a href="#appointment">My Appointments</a><a href="#reviews">Reviews</a></nav>
        <div className="home-account">
          {isAuthenticated ? <><div className="home-avatar" aria-hidden="true">{displayName.charAt(0).toUpperCase()}</div><div className="home-user"><strong>{displayName}</strong><span>{user?.role ?? "Customer"}</span></div><button type="button" className="home-logout" onClick={logout}>Log out</button></> : <div className="home-auth-links"><Link to="/login">Log in</Link><Link to="/register">Sign up</Link></div>}
        </div>
      </header>
      <div className="home-content" id="home">
        <section className="home-hero" aria-labelledby="home-heading">
          <div className="home-hero-copy"><p className="home-eyebrow">NAIL SALON BOOKING MADE SIMPLE</p><h1 id="home-heading">Find your next nail look.<br />Book it in minutes.</h1><p>Browse designs, compare services, choose a time, and manage your appointments in one place.</p><div className="home-hero-actions"><a className="home-primary-button" href="#services">Book appointment</a><a className="home-secondary-button" href="#designs">Browse designs</a></div></div>
          <div className="home-hero-art" aria-hidden="true"><div className="home-hero-blue-shape" /><img src="https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=1200&q=88" alt="" /></div>
        </section>
        <section className="home-discovery" aria-label="Find nail designs"><label className="home-search-field" htmlFor="nail-search"><span>⌕</span><input id="nail-search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search nail designs by name..." /></label><div className="home-categories"><span>Categories</span><div>{mockCategories.map((category) => <button type="button" key={category} className={category === selectedCategory ? "is-selected" : ""} onClick={() => setSelectedCategory(category)}>{category}</button>)}</div></div></section>
        <section className="home-designs" id="designs" aria-labelledby="designs-heading"><div className="home-section-heading"><div><h2 id="designs-heading">Featured nail designs</h2><p>Search and filter by style such as French, Chrome, Minimal, Korean and Luxury.</p></div><a href="#designs">View all designs <span>&rarr;</span></a></div>{filteredDesigns.length ? <div className="home-design-grid">{filteredDesigns.slice(0, 4).map((design) => <article className="home-design-card" key={design.id}><img src={design.imageUrl} alt={`${design.name} nail design`} /><div><h3>{design.name}</h3><strong>+${design.extraPrice}</strong><p>{design.category}</p></div></article>)}</div> : <p className="home-empty-state">No nail designs found. Try another search or category.</p>}</section>
        <section className={`home-bottom-grid ${isAuthenticated ? "" : "home-bottom-grid-guest"}`}>
          <section className="home-services" id="services" aria-labelledby="services-heading"><div className="home-section-heading"><h2 id="services-heading">Popular services</h2><a href="#services">View services <span>&rarr;</span></a></div><div>{mockServices.map((service) => <article className="home-service-row" key={service.id}><span className="home-service-icon">{service.icon}</span><div><h3>{service.name}</h3><p>{service.durationMinutes} min</p></div><strong>${service.price}</strong></article>)}</div></section>
          {isAuthenticated && <section className="home-appointment" id="appointment" aria-labelledby="appointment-heading"><p>UPCOMING APPOINTMENT</p><h2 id="appointment-heading">{mockUpcomingAppointment.dateLabel}</h2><span>{mockUpcomingAppointment.serviceName} &bull; {mockUpcomingAppointment.durationMinutes} min</span><strong>{mockUpcomingAppointment.status}</strong><div><button type="button">Manage appointment</button><small>Need to change plans? View or cancel your booking.</small></div></section>}
        </section>
      </div>
    </main>
  );
}
