import { create } from "zustand";
import type { Service } from "@/types/service";
interface State { items: Service[]; toggle: (service: Service) => void; clear: () => void; }
export const useServiceSelectionStore = create<State>((set) => ({ items: [], toggle: (service) => set((state) => state.items.some((x) => x.id === service.id) ? { items: state.items.filter((x) => x.id !== service.id) } : { items: [...state.items, service] }), clear: () => set({ items: [] }) }));
