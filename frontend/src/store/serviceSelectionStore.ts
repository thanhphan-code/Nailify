import { create } from "zustand";
import type { Service } from "@/types/service";
interface State { items: Service[]; select: (service: Service) => void; clear: () => void; }
export const useServiceSelectionStore = create<State>((set) => ({ items: [], select: (service) => set({ items: [service] }), clear: () => set({ items: [] }) }));
