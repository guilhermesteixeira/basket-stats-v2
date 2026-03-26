import { useQuery } from '@tanstack/react-query'
import { listTeams } from '../api/teams'
import type { Team } from '../types'

export function useTeams() {
  return useQuery<Team[]>({
    queryKey: ['teams'],
    queryFn: listTeams,
    staleTime: 60_000,
  })
}
