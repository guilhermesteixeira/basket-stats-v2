import { useQuery } from '@tanstack/react-query'
import { getMatch } from '../api/matches'
import { db } from '../offline/db'
import type { Match } from '../types'

export function useMatch(id: string) {
  return useQuery<Match>({
    queryKey: ['matches', id],
    queryFn: async () => {
      const match = await getMatch(id)
      await db.cachedMatches.put({ matchId: id, data: match, updatedAt: Date.now() })
      return match
    },
    staleTime: 10_000,
  })
}
