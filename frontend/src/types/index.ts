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
  coordinates?: Coordinates
  made?: boolean
  foulType?: 'personal' | 'technical' | 'flagrant'
  playerName?: string
  playerFouled?: string
  flagrant?: boolean
  playerIn?: string
  playerOut?: string
}

export interface MatchEvent {
  id: string
  timestamp: string
  periodNumber: number
  periodTimestamp: number
  type: EventType
  teamId: string
  playerId?: string
  details: EventDetails
}

export interface Period {
  number: number
  startTime?: string
  endTime?: string
  duration?: number
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
  type: EventType
  teamId: string
  periodNumber: number
  periodTimestamp: number
  playerId?: string
  details: EventDetails
}

export interface UserProfile {
  id: string
}
