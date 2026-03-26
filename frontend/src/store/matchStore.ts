import { create } from 'zustand'

interface MatchStoreState {
  activeMatchId: string | null
  currentPeriod: number
  setActiveMatch: (id: string) => void
  setCurrentPeriod: (period: number) => void
}

export const useMatchStore = create<MatchStoreState>((set) => ({
  activeMatchId: null,
  currentPeriod: 1,
  setActiveMatch: (id) => set({ activeMatchId: id }),
  setCurrentPeriod: (period) => set({ currentPeriod: period }),
}))
