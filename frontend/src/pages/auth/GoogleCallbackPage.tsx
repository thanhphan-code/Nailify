import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import * as authApi from "@/features/auth/api";
import { useAuthStore } from "@/store/authStore";

export default function GoogleCallbackPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const ticket = params.get("ticket");
    if (!ticket) { setError("Google sign-in session is missing."); return; }
    authApi.exchangeGoogleTicket(ticket)
      .then((response) => { setAuth(response); navigate("/", { replace: true }); })
      .catch(() => setError("Google sign-in could not be completed. Please try again."));
  }, [navigate, params, setAuth]);

  return <main className="auth-shell"><section className="auth-panel auth-status-panel"><h1>{error ? "Sign-in failed" : "Signing you in..."}</h1><p>{error ?? "Please wait while we securely complete Google sign-in."}</p>{error && <button type="button" className="auth-submit" onClick={() => navigate("/login", { replace: true })}>Back to login</button>}</section></main>;
}
