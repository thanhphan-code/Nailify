// Mirrors SRS section 11.6 - NailDesign entity
export interface NailDesign {
  id: string;
  name: string;
  imageUrl: string;
  categoryId: string;
  extraPrice: number;
  status: "Active" | "Inactive";
}
