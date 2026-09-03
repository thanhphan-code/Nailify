import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import * as authApi from "@/features/auth/api";

type Step = "email" | "code" | "password" | "complete";

export default function ForgotPasswordPage() {
  const [step, setStep] = useState<Step>("email");
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [password, setPassword] = useState("");
  const [resetToken, setResetToken] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const submit = async (action: () => Promise<void>) => { setError(null); setLoading(true); try { await action(); } catch (requestError) { setError((requestError as { response?: { data?: { title?: string } } }).response?.data?.title ?? "Something went wrong. Please try again."); } finally { setLoading(false); } };

  return <main className="auth-shell"><section className="auth-panel auth-reset-panel"><Link to="/login" className="auth-brand">Nailify</Link>
    {step === "email" && <><div className="auth-heading"><h1>Forgot your password?</h1><p>Enter your email and we will send a 6-digit verification code.</p></div><form className="auth-form" onSubmit={(event) => { event.preventDefault(); submit(async () => { await authApi.requestPasswordReset(email); setStep("code"); }); }}><div className="auth-field"><label htmlFor="reset-email">Email</label><input id="reset-email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Enter your email" required autoComplete="email" /></div>{error && <p className="form-error">{error}</p>}<button className="auth-submit" disabled={loading}>{loading ? "Sending code..." : "Send verification code"}</button></form></>}
    {step === "code" && <><div className="auth-heading"><h1>Check your inbox</h1><p>We sent a 6-digit code to {email}.</p></div><form className="auth-form" onSubmit={(event) => { event.preventDefault(); submit(async () => { const token = await authApi.verifyPasswordResetCode(email, code); setResetToken(token); setStep("password"); }); }}><div className="auth-field"><label htmlFor="reset-code">Verification code</label><input id="reset-code" inputMode="numeric" pattern="[0-9]{6}" maxLength={6} value={code} onChange={(event) => setCode(event.target.value.replace(/\D/g, ""))} placeholder="000000" required /></div>{error && <p className="form-error">{error}</p>}<button className="auth-submit" disabled={loading}>{loading ? "Verifying..." : "Verify code"}</button></form></>}
    {step === "password" && <><div className="auth-heading"><h1>Create a new password</h1><p>Use 8+ characters including uppercase, lowercase, number and special character.</p></div><form className="auth-form" onSubmit={(event) => { event.preventDefault(); submit(async () => { await authApi.confirmPasswordReset(resetToken, password); setStep("complete"); }); }}><div className="auth-field"><label htmlFor="new-password">New password</label><input id="new-password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Enter a new password" required autoComplete="new-password" /></div>{error && <p className="form-error">{error}</p>}<button className="auth-submit" disabled={loading}>{loading ? "Updating password..." : "Update password"}</button></form></>}
    {step === "complete" && <div className="auth-heading"><h1>Password updated</h1><p>Your password has been updated. You can now sign in.</p><button type="button" className="auth-submit" onClick={() => navigate("/login")}>Back to login</button></div>}
  </section></main>;
}
