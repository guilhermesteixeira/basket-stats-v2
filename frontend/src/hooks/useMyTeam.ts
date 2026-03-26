import { useTeams } from './useTeams'
import { useAuthStore } from '../auth/AuthProvider'
import type { Team } from '../types'

export function useMyTeam(): Team | undefined {
  const { data: teams } = useTeams()
  const { internalUserId } = useAuthStore()
  return teams?.find((t) => t.ownerId === internalUserId)
}
