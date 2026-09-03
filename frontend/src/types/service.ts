export interface Service { id: string; name: string; category: string; price: number; durationMinutes: number; description?: string | null; imageUrl?: string | null; isActive: boolean; }
export interface ServiceListResponse { items: Service[]; totalCount: number; page: number; pageSize: number; }
