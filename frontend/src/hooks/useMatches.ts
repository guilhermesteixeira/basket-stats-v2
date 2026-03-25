import { useQuery } from '@tanstack/react-query'
import { listMatches } from '../api/matches'
import type { Match } from '../types'

export function useMatches() {
  return useQuery<Match[]>({
    queryKey: ['matches'],
    queryFn: listMatches,
    staleTime: 30_000,
  })
}
