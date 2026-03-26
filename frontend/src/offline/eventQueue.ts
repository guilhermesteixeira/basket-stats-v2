import { db } from './db'
import { addEvent } from '../api/matches'
import type { AddEventRequest } from '../types'

export async function enqueueEvent(matchId: string, payload: AddEventRequest): Promise<void> {
  await db.offlineQueue.add({
    matchId,
    payload,
    createdAt: Date.now(),
    status: 'pending',
  })
}

export async function flushQueue(matchId: string): Promise<{ synced: number; failed: number }> {
  const items = await db.offlineQueue
    .where('matchId')
    .equals(matchId)
    .and((item) => item.status === 'pending')
    .toArray()

  let synced = 0
  let failed = 0

  for (const item of items) {
    if (item.id === undefined) continue

    await db.offlineQueue.update(item.id, { status: 'syncing' })
    try {
      await addEvent(matchId, item.payload)
      await db.offlineQueue.delete(item.id)
      synced++
    } catch {
      await db.offlineQueue.update(item.id, { status: 'failed' })
      failed++
    }
  }

  return { synced, failed }
}

export async function getPendingCount(matchId: string): Promise<number> {
  return db.offlineQueue
    .where('matchId')
    .equals(matchId)
    .and((item) => item.status === 'pending' || item.status === 'syncing')
    .count()
}
