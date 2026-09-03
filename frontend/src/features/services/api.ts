import { apiClient } from "@/services/apiClient";
import type { ServiceListResponse } from "@/types/service";
export async function getServices(params: Record<string, string | number | undefined>) { const { data } = await apiClient.get<ServiceListResponse>("/services", { params }); return data; }
