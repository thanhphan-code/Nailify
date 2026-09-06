import { Link } from "react-router-dom";
import AccountMenu from "@/components/AccountMenu";
import { useAuth } from "@/hooks/useAuth";

type CustomerSection = "home" | "designs" | "services" | "appointments";

export default function CustomerHeader({ active }: { active: CustomerSection }) {
  const { isAuthenticated } = useAuth();
  return (
    <header className="home-header customer-header">
      <Link to="/" className="home-logo" aria-label="Nailify - Trang chủ">Nailify<span>.</span></Link>
      <nav className="home-nav" aria-label="Điều hướng chính">
        <Link className={active === "home" ? "is-active" : undefined} to="/">Trang chủ</Link>
        <Link className={active === "designs" ? "is-active" : undefined} to="/nail-designs">Mẫu nail</Link>
        <Link className={active === "services" ? "is-active" : undefined} to="/services">Dịch vụ</Link>
        {isAuthenticated && <Link className={active === "appointments" ? "is-active" : undefined} to="/appointments">Lịch hẹn của tôi</Link>}
        <Link to={{ pathname: "/", hash: "#reviews" }}>Đánh giá</Link>
      </nav>
      <div className="home-account">
        {isAuthenticated ? <AccountMenu /> : <div className="home-auth-links"><Link to="/login">Đăng nhập</Link><Link to="/register">Đăng ký</Link></div>}
      </div>
    </header>
  );
}
