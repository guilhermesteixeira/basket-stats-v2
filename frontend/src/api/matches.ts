import type { Match, MatchEvent, CreateMatchRequest, AddEventRequest } from '../types'
import { apiClient } from './client'

export async function listMatches(): Promise<Match[]> {
  const { data } = await apiClient.get<Match[]>('/api/matches')
  return data
}

export async function getMatch(id: string): Promise<Match> {
  const { data } = await apiClient.get<Match>(`/api/matches/${id}`)
  return data
}

export async function createMatch(req: CreateMatchRequest): Promise<Match> {
  const { data } = await apiClient.post<Match>('/api/matches', req)
  return data
}

export async function startMatch(id: string): Promise<Match> {
  const { data } = await apiClient.post<Match>(`/api/matches/${id}/start`)
  return data
}

export async function finishMatch(id: string): Promise<Match> {
  const { data } = await apiClient.post<Match>(`/api/matches/${id}/finish`)
  return data
}

export async function addEvent(matchId: string, req: AddEventRequest): Promise<MatchEvent> {
  const { data } = await apiClient.post<MatchEvent>(`/api/matches/${matchId}/events`, req)
  return data
}
