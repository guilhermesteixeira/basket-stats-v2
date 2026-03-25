import { useEffect, useRef, useState } from 'react'
import { useNetworkStatus } from './useNetworkStatus'
import { flushQueue, getPendingCount } from '../offline/eventQueue'

export function useSyncQueue(matchId: string) {
  const { online } = useNetworkStatus()
  const prevOnlineRef = useRef(online)
  const [pendingCount, setPendingCount] = useState(0)
  const [isSyncing, setIsSyncing] = useState(false)

  const refreshCount = () => {
    getPendingCount(matchId).then(setPendingCount).catch(console.error)
  }

  useEffect(() => {
    refreshCount()
    const interval = setInterval(refreshCount, 5_000)
    return () => clearInterval(interval)
  }, [matchId])

  useEffect(() => {
    if (online && !prevOnlineRef.current) {
      setIsSyncing(true)
      flushQueue(matchId)
        .then(() => refreshCount())
        .catch(console.error)
        .finally(() => setIsSyncing(false))
    }
    prevOnlineRef.current = online
  }, [online, matchId])

  return { pendingCount, isSyncing }
}
