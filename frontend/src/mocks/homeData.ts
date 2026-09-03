export const mockCategories = ["All", "French", "Chrome", "Minimal", "Korean", "Luxury"] as const;

export type MockNailCategory = Exclude<(typeof mockCategories)[number], "All">;

export interface MockNailDesign {
  id: string;
  name: string;
  category: MockNailCategory;
  extraPrice: number;
  imageUrl: string;
}

export interface MockService {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
  icon: string;
}

export interface MockUpcomingAppointment {
  id: string;
  dateLabel: string;
  serviceName: string;
  durationMinutes: number;
  status: "Confirmed";
}

export const mockNailDesigns: MockNailDesign[] = [
  { id: "design-soft-french", name: "Soft French", category: "French", extraPrice: 8, imageUrl: "https://images.fresha.com/locations/location-profile-images/500430/5594557/68a7e831-2253-4cbd-a2d8-3c10b834927e-LaurasNailBoutique-GB-England-SilverEnd-Fresha.jpg" },
  { id: "design-ocean-chrome", name: "Ocean Chrome", category: "Chrome", extraPrice: 12, imageUrl: "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=900&q=85" },
  { id: "design-clean-minimal", name: "Clean Minimal", category: "Minimal", extraPrice: 6, imageUrl: "https://i.pinimg.com/originals/94/e8/60/94e86009ff6714bb303859b5f8338807.jpg" },
  { id: "design-korean-glow", name: "Korean Glow", category: "Korean", extraPrice: 10, imageUrl: "https://images.fresha.com/locations/location-profile-images/2702613/5411632/aafbe2e8-c4d0-4a44-b2f0-b6e71866bbeb-DanaNailsMtl-CA-Canada-Montral-Westmount-Fresha.jpg" },
  { id: "design-pearl-luxe", name: "Pearl Luxe", category: "Luxury", extraPrice: 18, imageUrl: "https://static.wixstatic.com/media/14f7dd_341c966a1b0d4e1dbb6562c932dcb0fd~mv2.jpg/v1/fit/w_2500,h_1330,al_c/14f7dd_341c966a1b0d4e1dbb6562c932dcb0fd~mv2.jpg" },
  { id: "design-french-blush", name: "French Blush", category: "French", extraPrice: 9, imageUrl: "https://images.fresha.com/locations/location-profile-images/1244916/5085793/1a3cbcb6-28a4-403d-9141-40a6598424ee-ChicNailStudio-US-Virginia-Arlington-Barcroft-Fresha.jpg?class=venue-gallery-small&f_width=3840" },
];

export const mockServices: MockService[] = [
  { id: "service-classic-manicure", name: "Classic Manicure", durationMinutes: 45, price: 25, icon: "✦" },
  { id: "service-gel-manicure", name: "Gel Manicure", durationMinutes: 60, price: 38, icon: "●" },
  { id: "service-nail-art-addon", name: "Nail Art Add-on", durationMinutes: 30, price: 15, icon: "✧" },
];

export const mockUpcomingAppointment: MockUpcomingAppointment = {
  id: "appointment-2026-08-29-1530",
  dateLabel: "Sat, Aug 29 • 3:30 PM",
  serviceName: "Gel Manicure",
  durationMinutes: 60,
  status: "Confirmed",
};
