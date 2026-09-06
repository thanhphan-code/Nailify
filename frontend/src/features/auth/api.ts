import { apiClient } from "@/services/apiClient";
import type {
  AuthResponse,
  LoginPayload,
  RegisterPayload,
  User,
} from "@/types/auth";

function mapUser(dto: {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  status: string;
}): User {
  return {
    id: dto.id,
    fullName: dto.fullName,
    email: dto.email,
    phoneNumber: dto.phoneNumber,
    role: dto.role as User["role"],
    status: dto.status,
  };
}

function mapAuthResponse(data: {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: {
    id: string;
    fullName: string;
    email: string;
    phoneNumber: string;
    role: string;
    status: string;
  };
}): AuthResponse {
  return {
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    accessTokenExpiresAt: data.accessTokenExpiresAt,
    user: mapUser(data.user),
  };
}

export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const { data } = await apiClient.post("/auth/register", payload);
  return mapAuthResponse(data);
}

export async function login(payload: LoginPayload): Promise<AuthResponse> {
  const { data } = await apiClient.post("/auth/login", payload);
  return mapAuthResponse(data);
}

export async function refresh(refreshToken: string): Promise<AuthResponse> {
  const { data } = await apiClient.post("/auth/refresh-token", { refreshToken });
  return mapAuthResponse(data);
}

export async function getCurrentUser(): Promise<User> {
  const { data } = await apiClient.get("/auth/me");
  return mapUser(data);
}

export async function logout(refreshToken: string): Promise<void> {
  await apiClient.post("/auth/logout", { refreshToken });
}

export async function exchangeGoogleTicket(ticket: string): Promise<AuthResponse> {
  const { data } = await apiClient.post("/auth/google/exchange", { ticket });
  return mapAuthResponse(data);
}

export async function requestPasswordReset(email: string): Promise<void> {
  await apiClient.post("/auth/password-reset/request", { email });
}

export async function verifyPasswordResetCode(email: string, code: string): Promise<string> {
  const { data } = await apiClient.post<{ resetToken: string }>("/auth/password-reset/verify", { email, code });
  return data.resetToken;
}

export async function confirmPasswordReset(resetToken: string, newPassword: string): Promise<void> {
  await apiClient.post("/auth/password-reset/confirm", { resetToken, newPassword });
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  await apiClient.post("/auth/change-password", { currentPassword, newPassword });
}
