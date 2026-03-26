export type MatchStatus = 'Scheduled' | 'Active' | 'Finished'
export type EventType = 'Score' | 'MissedShot' | 'FreeThrow' | 'Foul' | 'Substitution'

export interface Coordinates {
  x: number
  y: number
}

export interface Team {
  id: string
  name: string
  ownerId: string
}

export interface MatchTeam {
  teamId: string
  teamName: string
}

export interface EventDetails {
  points?: number
  made?: boolean
  foulType?: string
  playerId?: string
  playerFouledId?: string
  flagrant?: boolean
  playerInId?: string
  playerOutId?: string
}

export interface MatchEvent {
  id: string
  teamId: string
  playerId?: string
  type: string
  timestamp: string
  periodNumber: number
  periodTimestamp: number
  points?: number
  coordinatesX?: number
  coordinatesY?: number
  made?: boolean
  foulType?: string
  playerFouledId?: string
  flagrant?: boolean
  playerOutId?: string
}

export interface Period {
  number: number
  startTime?: string
  endTime?: string
  durationSeconds?: number
}

export interface Match {
  id: string
  homeTeam: MatchTeam
  awayTeam: MatchTeam
  status: MatchStatus
  createdAt: string
  startedAt?: string
  finishedAt?: string
  events: MatchEvent[]
  periods: Period[]
  homeScore: number
  awayScore: number
}

export interface CreateMatchRequest {
  homeTeamId: string
  awayTeamId: string
}

export interface CreateTeamRequest {
  name: string
}

export interface AddEventRequest {
  type: string
  teamId: string
  periodNumber: number
  periodTimestamp: number
  playerId?: string
  coordinatesX?: number
  coordinatesY?: number
  points?: number
  made?: boolean
  foulType?: string
  playerFouledId?: string
  flagrant?: boolean
  playerOutId?: string
}

export interface UserProfile {
  id: string
}
