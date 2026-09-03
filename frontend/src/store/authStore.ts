import { create } from "zustand";
import type { AuthResponse, User } from "@/types/auth";
import { clearTokens, setTokens } from "@/services/tokenStorage";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  setAuth: (response: AuthResponse) => void;
  setUser: (user: User | null) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  setAuth: (response) => {
    setTokens(response.accessToken, response.refreshToken);
    set({
      user: response.user,
      isAuthenticated: true,
    });
  },
  setUser: (user) =>
    set({
      user,
      isAuthenticated: user !== null,
    }),
  logout: () => {
    clearTokens();
    set({ user: null, isAuthenticated: false });
  },
}));
