import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import * as authApi from "@/features/auth/api";
import { useAuthStore } from "@/store/authStore";
import { getRefreshToken } from "@/services/tokenStorage";
import type { LoginPayload, RegisterPayload } from "@/types/auth";

export function useAuth() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const setAuth = useAuthStore((s) => s.setAuth);
  const setUser = useAuthStore((s) => s.setUser);
  const logoutStore = useAuthStore((s) => s.logout);

  const login = useCallback(
    async (payload: LoginPayload) => {
      const response = await authApi.login(payload);
      setAuth(response);
      return response;
    },
    [setAuth]
  );

  const register = useCallback(
    async (payload: RegisterPayload) => {
      const response = await authApi.register(payload);
      setAuth(response);
      return response;
    },
    [setAuth]
  );

  const logout = useCallback(async () => {
    const refreshToken = getRefreshToken();
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } finally {
      logoutStore();
      navigate("/login");
    }
  }, [logoutStore, navigate]);

  const loadCurrentUser = useCallback(async () => {
    const currentUser = await authApi.getCurrentUser();
    setUser(currentUser);
    return currentUser;
  }, [setUser]);

  return {
    user,
    isAuthenticated,
    login,
    register,
    logout,
    loadCurrentUser,
  };
}
