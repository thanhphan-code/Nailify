import { apiClient } from "@/services/apiClient";
import type { CustomerAppointment } from "@/features/bookings/api";

export async function getStaffAppointments() { const { data } = await apiClient.get<CustomerAppointment[]>("/staff/appointments"); return data; }
export async function updateAppointmentStatus(id: string, action: "confirm" | "start" | "complete") { const { data } = await apiClient.patch(`/staff/appointments/${id}/${action}`); return data; }
export async function staffCancelAppointment(id: string, reason: string) { const { data } = await apiClient.patch(`/staff/appointments/${id}/cancel`, { reason }); return data; }
