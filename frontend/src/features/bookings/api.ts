import { apiClient } from "@/services/apiClient";

// Mirrors CompatibleNailDesignDto returned by GET /api/services/{serviceId}/nail-designs.
// Keep the API's field names so the selected design ID and backend-calculated total cannot drift.
export interface CompatibleDesign {
  nailDesignId: string;
  name: string;
  imageUrl?: string | null;
  extraPrice: number;
  estimatedTotal: number;
  category: { name: string };
}
export interface BookingStaff {
  id: string;
  fullName: string;
  specialty?: string | null;
}
export interface BookingSlot {
  startTime: string;
  endTime: string;
}
export interface AvailableSlots {
  date: string;
  durationMinutes: number;
  availableSlots: BookingSlot[];
}
export interface CustomerAppointment {
  id: string;
  appointmentCode: string;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  status: string;
  serviceId: string;
  serviceName: string;
  nailDesignId?: string | null;
  nailDesignName?: string | null;
  staffId?: string | null;
  staffName?: string | null;
  servicePrice: number;
  designExtraPrice: number;
  totalPrice: number;
  note?: string | null;
  cancellationReason?: string | null;
  createdAt: string;
  services?: AppointmentServiceReview[];
  deposit?: DepositPayment | null;
}
export interface DepositPayment { id: string; amount: number; status: string; expiresAt: string; receiptUrl?: string | null; qrUrl: string; transferContent: string; checkoutUrl?: string | null; }
export interface ServiceReview { id: string; rating: number; comment?: string | null; isApproved: boolean; createdAt: string; }
export interface AppointmentServiceReview { serviceId: string; serviceName: string; price: number; durationMinutes: number; review?: ServiceReview | null; }

export async function getCompatibleDesigns(serviceId: string) {
  const { data } = await apiClient.get<CompatibleDesign[]>(
    `/services/${serviceId}/nail-designs`,
  );
  return data;
}
export async function getBookingStaff(serviceId: string) {
  const { data } = await apiClient.get<BookingStaff[]>(
    `/services/${serviceId}/staff`,
  );
  return data;
}
export async function getAvailableSlots(
  serviceId: string,
  nailDesignId: string,
  date: string,
  staffId?: string,
) {
  const { data } = await apiClient.get<AvailableSlots>(
    "/bookings/available-slots",
    {
      params: { serviceId, nailDesignId, date, staffId: staffId || undefined },
    },
  );
  return data;
}
export async function createAppointment(payload: {
  serviceId: string;
  nailDesignId: string;
  staffId?: string;
  appointmentDate: string;
  startTime: string;
  note?: string;
}) {
  const { data } = await apiClient.post<{ id: string }>("/appointments", payload);
  return data;
}
export async function getDeposit(appointmentId: string) { const { data } = await apiClient.get<DepositPayment>(`/deposits/${appointmentId}`); return data; }
export async function cancelDeposit(appointmentId: string) { await apiClient.post(`/deposits/${appointmentId}/cancel`); }
export async function getMyAppointments() {
  const { data } = await apiClient.get<CustomerAppointment[]>("/appointments");
  return data;
}
export async function cancelAppointment(id: string, reason?: string) {
  const { data } = await apiClient.patch(`/appointments/${id}/cancel`, {
    reason: reason?.trim() || undefined,
  });
  return data;
}
export async function rescheduleAppointment(
  id: string,
  payload: { staffId?: string; appointmentDate: string; startTime: string },
) {
  const { data } = await apiClient.patch(
    `/appointments/${id}/reschedule`,
    payload,
  );
  return data;
}
export async function createServiceReview(appointmentId: string, serviceId: string, rating: number, comment: string) {
  const { data } = await apiClient.post<ServiceReview>(`/appointments/${appointmentId}/services/${serviceId}/review`, { rating, comment: comment.trim() || undefined });
  return data;
}
