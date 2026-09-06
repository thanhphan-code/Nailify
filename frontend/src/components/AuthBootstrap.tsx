import { useEffect } from "react";
import { getAccessToken } from "@/services/tokenStorage";
import { useAuthStore } from "@/store/authStore";
import * as authApi from "@/features/auth/api";

export default function AuthBootstrap({ children }: { children: React.ReactNode }) {
  const user = useAuthStore((s) => s.user);
  const setUser = useAuthStore((s) => s.setUser);
  const logout = useAuthStore((s) => s.logout);

  useEffect(() => {
    if (!getAccessToken()) {
      if (user) logout();
      return;
    }
    if (user) return;

    authApi
      .getCurrentUser()
      .then(setUser)
      .catch(() => logout());
  }, [user, setUser, logout]);

  return <>{children}</>;
}
