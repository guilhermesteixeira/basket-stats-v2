import type { Team, CreateTeamRequest } from '../types'
import { apiClient } from './client'

export async function listTeams(): Promise<Team[]> {
  const { data } = await apiClient.get<Team[]>('/api/teams')
  return data
}

export async function createTeam(req: CreateTeamRequest): Promise<Team> {
  const { data } = await apiClient.post<Team>('/api/teams', req)
  return data
}
