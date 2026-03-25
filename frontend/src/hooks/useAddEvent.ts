import { useMutation, useQueryClient } from '@tanstack/react-query'
import { addEvent } from '../api/matches'
import { enqueueEvent } from '../offline/eventQueue'
import { useNetworkStatus } from './useNetworkStatus'
import type { AddEventRequest } from '../types'

export function useAddEvent(matchId: string) {
  const queryClient = useQueryClient()
  const { online } = useNetworkStatus()

  return useMutation({
    mutationFn: async (req: AddEventRequest) => {
      if (online) {
        const event = await addEvent(matchId, req)
        return event
      } else {
        await enqueueEvent(matchId, req)
        return null
      }
    },
    onSuccess: (_data, _variables) => {
      if (online) {
        void queryClient.invalidateQueries({ queryKey: ['matches', matchId] })
      }
    },
  })
}
