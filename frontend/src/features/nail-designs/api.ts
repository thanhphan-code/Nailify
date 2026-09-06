import { apiClient } from "@/services/apiClient";

export interface NailDesignListItem {
  id: string; name: string; imageUrl: string;
  category: { id: string; name: string; slug: string };
  additionalPrice: number; additionalDurationMinutes: number; isBookable: boolean; isFavorite: boolean;
}
export interface NailDesignListResponse { items: NailDesignListItem[]; pagination: { totalItems: number; }; }
export interface NailDesignCategory { id: string; name: string; slug: string; designCount: number; }
interface NailDesignCategoryListResponse { items: NailDesignCategory[]; }
export async function getNailDesigns(params?: { category?: string; search?: string; sort?: string; pageSize?: number }) {
  const { data } = await apiClient.get<NailDesignListResponse>("/nail-designs", { params });
  return data;
}
export async function getNailDesignCategories() {
  const { data } = await apiClient.get<NailDesignCategoryListResponse>("/nail-design-categories");
  return data.items;
}
