import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useState } from "react";
import type { AxiosError } from "axios";
import { useAuth } from "@/hooks/useAuth";

const passwordRule = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;
const registerSchema = z.object({
  fullName: z.string().trim().min(1, "Vui lòng nhập họ và tên").max(100, "Họ và tên tối đa 100 ký tự"),
  email: z.string().trim().email("Email không hợp lệ"),
  phoneNumber: z.string().trim().min(1, "Vui lòng nhập số điện thoại").max(30, "Số điện thoại tối đa 30 ký tự").regex(/^[0-9+() .-]+$/, "Số điện thoại không hợp lệ"),
  password: z.string().regex(passwordRule, "Mật khẩu cần 8+ ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt"),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, { message: "Xác nhận mật khẩu không khớp", path: ["confirmPassword"] });

type RegisterForm = z.infer<typeof registerSchema>;

type RegisterFormProps = {
  embedded?: boolean;
  onSwitchToLogin?: () => void;
};

export function RegisterForm({ embedded = false, onSwitchToLogin }: RegisterFormProps) {
  const navigate = useNavigate();
  const { register: registerUser } = useAuth();
  const [formError, setFormError] = useState<string | null>(null);
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RegisterForm>();

  const onSubmit = async (data: RegisterForm) => {
    setFormError(null);
    const parsed = registerSchema.safeParse(data);
    if (!parsed.success) return;
    try {
      const { confirmPassword: _confirmPassword, ...payload } = parsed.data;
      await registerUser(payload);
      navigate("/");
    } catch (error) {
      const axiosError = error as AxiosError<{ title?: string }>;
      setFormError(axiosError.response?.data?.title ?? "Không thể đăng ký. Vui lòng thử lại.");
    }
  };

  const formContent = (
    <>
        <div className="auth-heading">
          <h1 id="register-heading">Tạo tài khoản miễn phí</h1>
          <p>Đặt lịch làm đẹp. Yêu chiều bộ móng của bạn.</p>
        </div>
        <form noValidate onSubmit={handleSubmit(onSubmit)} className="auth-form">
          <div className="auth-field">
            <label htmlFor="fullName">Họ và tên</label>
            <input id="fullName" type="text" placeholder="Enter your Full Name" autoComplete="name" aria-invalid={Boolean(errors.fullName)} {...register("fullName")} />
            {errors.fullName && <p className="field-error">{errors.fullName.message}</p>}
          </div>
          <div className="auth-field">
            <label htmlFor="register-email">Email</label>
            <input id="register-email" type="email" placeholder="Enter your Email" autoComplete="email" aria-invalid={Boolean(errors.email)} {...register("email")} />
            {errors.email && <p className="field-error">{errors.email.message}</p>}
          </div>
          <div className="auth-field">
            <label htmlFor="phoneNumber">Số điện thoại</label>
            <input id="phoneNumber" type="tel" placeholder="Enter your Phone Number" autoComplete="tel" aria-invalid={Boolean(errors.phoneNumber)} {...register("phoneNumber")} />
            {errors.phoneNumber && <p className="field-error">{errors.phoneNumber.message}</p>}
          </div>
          <div className="auth-field">
            <label htmlFor="register-password">Mật khẩu</label>
            <input id="register-password" type="password" placeholder="Create a Password" autoComplete="new-password" aria-describedby="password-help" aria-invalid={Boolean(errors.password)} {...register("password")} />
            <p id="password-help" className="field-hint">Dùng ít nhất 8 ký tự gồm chữ hoa, chữ thường, số và ký tự đặc biệt.</p>
            {errors.password && <p className="field-error">{errors.password.message}</p>}
          </div>
          <div className="auth-field">
            <label htmlFor="confirmPassword">Xác nhận mật khẩu</label>
            <input id="confirmPassword" type="password" placeholder="Confirm your Password" autoComplete="new-password" aria-invalid={Boolean(errors.confirmPassword)} {...register("confirmPassword")} />
            {errors.confirmPassword && <p className="field-error">{errors.confirmPassword.message}</p>}
          </div>
          {formError && <p className="form-error" role="alert">{formError}</p>}
          <button type="submit" disabled={isSubmitting} className="auth-submit">
            {isSubmitting ? "Creating account..." : "Create Account"}
          </button>
        </form>
        <p className="auth-switch">
          Already have an account? {embedded ? (
            <button type="button" className="auth-switch-link" onClick={onSwitchToLogin}>Đăng nhập</button>
          ) : (
            <Link to="/login">Đăng nhập</Link>
          )}
        </p>
        <div className="auth-divider" aria-hidden="true"><span>HOẶC TIẾP TỤC VỚI</span></div>
        <div className="auth-socials">
          <button type="button" className="auth-social-button" aria-label="Continue with Google" onClick={() => window.location.assign("/api/auth/google")}>
            <span className="auth-social-mark auth-social-google">G</span> Tiếp tục với Google
          </button>
        </div>
    </>
  );

  if (embedded) {
    return <div className="auth-register-content">{formContent}</div>;
  }

  return (
    <main className="auth-shell auth-shell-register">
      <section className="auth-panel" aria-labelledby="register-heading">
        <Link to="/" className="auth-brand">Nailify</Link>
        {formContent}
      </section>
    </main>
  );
}

export default function RegisterPage() {
  return <RegisterForm />;
}
