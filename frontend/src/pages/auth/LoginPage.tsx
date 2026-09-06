import { Link, useLocation, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useState } from "react";
import type { AxiosError } from "axios";
import { useAuth } from "@/hooks/useAuth";
import { RegisterForm } from "@/pages/auth/RegisterPage";

const loginSchema = z.object({
  email: z.string().trim().email("Email không hợp lệ"),
  password: z.string().min(1, "Vui lòng nhập mật khẩu"),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [formError, setFormError] = useState<string | null>(null);
  const [showRegister, setShowRegister] = useState(false);
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginForm>();

  const onSubmit = async (data: LoginForm) => {
    setFormError(null);
    const parsed = loginSchema.safeParse(data);
    if (!parsed.success) return;
    try {
      const response = await login(parsed.data);
      const returnUrl = (location.state as { returnUrl?: string } | null)?.returnUrl;
      const defaultUrl = response.user.role === "Customer" ? "/" : "/staff/appointments";
      navigate(returnUrl?.startsWith("/") ? returnUrl : defaultUrl, { replace: true });
    } catch (error) {
      const axiosError = error as AxiosError<{ title?: string }>;
      setFormError(axiosError.response?.data?.title ?? "Không thể đăng nhập. Vui lòng thử lại.");
    }
  };

  return (
    <main className="auth-shell">
      <div className="auth-flip-scene">
      <div className={`auth-card-inner${showRegister ? " is-flipped" : ""}`}>
      <section className="auth-panel auth-login-panel auth-card-face" aria-labelledby="login-heading" aria-hidden={showRegister}>
        <aside className="auth-art" aria-label="Nailify nail art showcase">
          <div className="auth-art-orb auth-art-orb-top" />
          <div className="auth-art-orb auth-art-orb-bottom" />
          <div className="auth-art-photo-wrap">
            <img
              className="auth-art-photo"
              src="https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=900&q=85"
              alt="Nail art manicure"
            />
          </div>
          <Link to="/" className="auth-art-tag">Bộ móng mới đang chờ bạn</Link>
        </aside>

        <div className="auth-content">
        <div className="auth-heading">
          <h1 id="login-heading">Chào mừng bạn trở lại</h1>
          <p>Đăng nhập để quản lý các lịch hẹn làm móng.</p>
        </div>
        <form noValidate onSubmit={handleSubmit(onSubmit)} className="auth-form">
          <div className="auth-field">
            <label htmlFor="email">Email</label>
            <input id="email" type="email" placeholder="Enter your Email" autoComplete="email" aria-invalid={Boolean(errors.email)} {...register("email")} />
            {errors.email && <p className="field-error">{errors.email.message}</p>}
          </div>
          <div className="auth-field">
            <label htmlFor="password">Mật khẩu</label>
            <input id="password" type="password" placeholder="Enter your Password" autoComplete="current-password" aria-invalid={Boolean(errors.password)} {...register("password")} />
            {errors.password && <p className="field-error">{errors.password.message}</p>}
          </div>
          <Link className="auth-forgot" to="/forgot-password">Quên mật khẩu?</Link>
          {formError && <p className="form-error" role="alert">{formError}</p>}
          <button type="submit" disabled={isSubmitting} className="auth-submit">
            {isSubmitting ? "Signing in..." : "Log in"}
          </button>
        </form>
        <p className="auth-switch">Bạn mới đến Nailify? <button type="button" className="auth-switch-link" onClick={() => setShowRegister(true)}>Tạo tài khoản</button></p>
        <div className="auth-divider" aria-hidden="true"><span>HOẶC TIẾP TỤC VỚI</span></div>
        <div className="auth-socials">
          <button type="button" className="auth-social-button" aria-label="Continue with Google" onClick={() => window.location.assign("/api/auth/google")}>
            <span className="auth-social-mark auth-social-google">G</span> Tiếp tục với Google
          </button>
        </div>
        </div>
      </section>
      <section className="auth-panel auth-login-panel auth-card-face auth-card-back" aria-labelledby="register-heading" aria-hidden={!showRegister}>
        <aside className="auth-art" aria-label="Nailify nail art showcase">
          <div className="auth-art-orb auth-art-orb-top" />
          <div className="auth-art-orb auth-art-orb-bottom" />
          <div className="auth-art-photo-wrap">
            <img
              className="auth-art-photo"
              src="https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=900&q=85"
              alt="Nail art manicure"
            />
          </div>
          <Link to="/" className="auth-art-tag">Bộ móng mới đang chờ bạn</Link>
        </aside>
        <div className="auth-content">
          <RegisterForm embedded onSwitchToLogin={() => setShowRegister(false)} />
        </div>
      </section>
      </div>
      </div>
    </main>
  );
}
