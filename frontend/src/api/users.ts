import type { UserProfile } from '../types'
import { apiClient } from './client'

export async function registerUser(): Promise<UserProfile> {
  const { data } = await apiClient.post<UserProfile>('/api/users/register')
  return data
}

export async function getMe(): Promise<UserProfile> {
  const { data } = await apiClient.get<UserProfile>('/api/users/me')
  return data
}
