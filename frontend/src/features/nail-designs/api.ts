import { apiClient } from "@/services/apiClient";
import type { NailDesign } from "@/types/nailDesign";

export async function getNailDesigns(params?: { categoryId?: string; search?: string }) {
  const { data } = await apiClient.get<NailDesign[]>("/nail-designs", { params });
  return data;
}
