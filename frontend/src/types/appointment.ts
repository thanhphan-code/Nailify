// Mirrors SRS section 7 - Appointment status + section 11.8
export type AppointmentStatus =
  | "Pending"
  | "Confirmed"
  | "InProgress"
  | "Completed"
  | "Cancelled";

export interface Appointment {
  id: string;
  customerId: string;
  staffId: string | null; // BR-010: staff is optional
  serviceId: string;
  nailDesignId: string;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  totalPrice: number;
  status: AppointmentStatus;
  note?: string;
}
