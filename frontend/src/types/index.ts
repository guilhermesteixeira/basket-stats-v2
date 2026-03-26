export type MatchStatus = 'Scheduled' | 'Active' | 'Finished'
export type EventType = 'Score' | 'MissedShot' | 'Foul' | 'Substitution' | 'Turnover'

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

export interface Player {
  id: string
  name: string
  number: number
}

export interface PlayerInput {
  name: string
  number: number
}

export interface EventDetails {
  points?: number
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
  homePlayers: Player[]
  awayPlayers: Player[]
}

export interface CreateMatchRequest {
  homeTeamId: string
  awayTeamId: string
  homePlayers?: PlayerInput[]
  awayPlayers?: PlayerInput[]
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
  foulType?: string
  playerFouledId?: string
  flagrant?: boolean
  playerOutId?: string
}

export interface UserProfile {
  id: string
}
