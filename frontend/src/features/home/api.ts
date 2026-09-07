import { apiClient } from "@/services/apiClient";

export interface HomeData {
  banner: { title: string; description: string; imageUrl?: string | null };
  categories: { id: string; name: string; slug: string; designCount: number }[];
  featuredDesigns: { id: string; name: string; imageUrl: string; categoryName: string; additionalPrice: number; additionalDurationMinutes: number; isFavorite: boolean }[];
  popularServices: { id: string; name: string; description?: string | null; durationMinutes: number; basePrice: number; imageUrl?: string | null }[];
  featuredReviews: { customerName: string; rating: number; comment?: string | null; serviceName: string; createdAt: string }[];
  salon: { name: string; phone: string; address: string; timezone: string; openingHours: string[] };
}

export async function getHome() {
  const { data } = await apiClient.get<HomeData>("/home", { params: { featuredDesignLimit: 8, serviceLimit: 4, reviewLimit: 6 } });
  return data;
}
