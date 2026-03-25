import Dexie, { type Table } from 'dexie'
import type { AddEventRequest, Match } from '../types'

export interface OfflineQueueItem {
  id?: number
  matchId: string
  payload: AddEventRequest
  createdAt: number
  status: 'pending' | 'syncing' | 'failed'
}

export interface CachedMatch {
  matchId: string
  data: Match
  updatedAt: number
}

class BasketStatsDB extends Dexie {
  offlineQueue!: Table<OfflineQueueItem, number>
  cachedMatches!: Table<CachedMatch, string>

  constructor() {
    super('BasketStatsDB')
    this.version(1).stores({
      offlineQueue: '++id, matchId, status',
      cachedMatches: 'matchId',
    })
  }
}

export const db = new BasketStatsDB()
