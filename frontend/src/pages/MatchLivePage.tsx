import { useParams, useNavigate, Link } from 'react-router-dom'
import { useMatch } from '../hooks/useMatch'
import { useAuthStore } from '../auth/AuthProvider'
import { useNetworkStatus } from '../hooks/useNetworkStatus'
import { useSyncQueue } from '../hooks/useSyncQueue'
import { startMatch, finishMatch, addEvent } from '../api/matches'
import { useQueryClient } from '@tanstack/react-query'
import { useState, useCallback, useRef } from 'react'
import { OfflineBanner } from '../components/offline/OfflineBanner'
import { GameClock } from '../components/domain/GameClock'
import { PlayerRosterPanel } from '../components/domain/PlayerRosterPanel'
import { PlayerActionButtons } from '../components/domain/PlayerActionButtons'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Spinner } from '../components/ui/Spinner'
import type { Player } from '../types'

export function MatchLivePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: match, isLoading } = useMatch(id ?? '')
  const { userRoles } = useAuthStore()
  const { online } = useNetworkStatus()
  const { pendingCount, isSyncing } = useSyncQueue(id ?? '')
  const isAdmin = userRoles.includes('admin') || userRoles.includes('match-creator')

  const [starting, setStarting] = useState(false)
  const [finishing, setFinishing] = useState(false)
  const [selectedPlayer, setSelectedPlayer] = useState<{ player: Player; teamId: string; color: 'blue' | 'orange' } | null>(null)
  const [actionFeedback, setActionFeedback] = useState<string | null>(null)

  // Track current clock time
  const clockRef = useRef({ periodNumber: 1, periodTimestamp: 0 })

  const handleTimeUpdate = useCallback((periodNumber: number, periodTimestamp: number) => {
    clockRef.current = { periodNumber, periodTimestamp }
  }, [])

  if (!id) {
    navigate('/')
    return null
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <Spinner size="lg" />
      </div>
    )
  }

  if (!match) {
    return <div className="text-center py-20 text-red-400">Match not found.</div>
  }

  const handleStart = async () => {
    setStarting(true)
    try {
      await startMatch(id)
      await queryClient.invalidateQueries({ queryKey: ['matches', id] })
    } finally {
      setStarting(false)
    }
  }

  const handleFinish = async () => {
    setFinishing(true)
    try {
      await finishMatch(id)
      await queryClient.invalidateQueries({ queryKey: ['matches', id] })
    } finally {
      setFinishing(false)
    }
  }

  const handleSelectPlayer = (player: Player, teamId: string, color: 'blue' | 'orange') => {
    if (selectedPlayer?.player.id === player.id) {
      setSelectedPlayer(null)
    } else {
      setSelectedPlayer({ player, teamId, color })
    }
  }

  const handleAction = async (action: {
    type: string
    teamId: string
    playerId: string
    points?: number
    coordinatesX?: number
    coordinatesY?: number
    foulType?: string
    flagrant?: boolean
    playerFouledId?: string
    playerOutId?: string
  }) => {
    try {
      await addEvent(id, {
        ...action,
        periodNumber: clockRef.current.periodNumber,
        periodTimestamp: clockRef.current.periodTimestamp,
      })
      await queryClient.invalidateQueries({ queryKey: ['matches', id] })

      // Find player name for feedback
      const playerName = selectedPlayer?.player.name ?? action.playerId
      const feedbackText = action.type === 'Score'
        ? `${playerName} +${action.points}pts`
        : action.type === 'Turnover'
          ? `${playerName} Turnover`
          : `${playerName} ${action.type}`
      setActionFeedback(feedbackText)
      setTimeout(() => setActionFeedback(null), 2000)
      setSelectedPlayer(null)
    } catch (err) {
      console.error('Failed to add event:', err)
      setActionFeedback('⚠ Failed to add event')
      setTimeout(() => setActionFeedback(null), 3000)
    }
  }

  // Resolve player names in events
  const allPlayers = [...(match.homePlayers ?? []), ...(match.awayPlayers ?? [])]
  const playerMap = new Map(allPlayers.map(p => [p.id, p]))

  const resolvePlayerName = (playerId?: string) => {
    if (!playerId) return ''
    const p = playerMap.get(playerId)
    return p ? `#${p.number} ${p.name}` : playerId
  }

  const recentEvents = [...match.events].reverse().slice(0, 10)

  return (
    <div className="max-w-7xl mx-auto px-4 py-4 space-y-3">
      {!online && <OfflineBanner pendingCount={pendingCount} />}

      {/* Scoreboard + Clock */}
      <div className="bg-slate-800 rounded-lg p-4">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-3">
            <Link to="/" className="text-slate-400 hover:text-white transition-colors text-sm">← Back</Link>
            <Badge status={match.status} />
            {isSyncing && (
              <span className="text-xs text-blue-400 flex items-center gap-1">
                <Spinner size="sm" /> Syncing...
              </span>
            )}
          </div>
          <div className="flex gap-2">
            <Link to={`/matches/${id}/stats`}>
              <Button variant="secondary" className="text-xs">📊 Stats</Button>
            </Link>
            {isAdmin && match.status === 'Scheduled' && (
              <Button onClick={handleStart} loading={starting} className="text-xs">▶ Start</Button>
            )}
            {isAdmin && match.status === 'Active' && (
              <Button variant="danger" onClick={handleFinish} loading={finishing} className="text-xs">⏹ Finish</Button>
            )}
          </div>
        </div>

        {/* Score + Clock row */}
        <div className="flex items-center justify-between">
          <div className="flex-1 text-center">
            <p className="text-blue-400 font-bold text-sm">{match.homeTeam.teamName}</p>
            <p className="text-5xl font-bold text-white mt-1">{match.homeScore}</p>
          </div>
          <div className="px-4">
            <GameClock
              matchStatus={match.status}
              periods={match.periods}
              onTimeUpdate={handleTimeUpdate}
            />
          </div>
          <div className="flex-1 text-center">
            <p className="text-orange-400 font-bold text-sm">{match.awayTeam.teamName}</p>
            <p className="text-5xl font-bold text-white mt-1">{match.awayScore}</p>
          </div>
        </div>
      </div>

      {/* Action feedback toast */}
      {actionFeedback && (
        <div className="bg-green-900/80 border border-green-600 rounded-lg px-4 py-2 text-center text-green-300 text-sm font-medium animate-pulse">
          {actionFeedback}
        </div>
      )}

      {/* Player action panel — when a player is selected */}
      {match.status === 'Active' && selectedPlayer && (
        <PlayerActionButtons
          player={selectedPlayer.player}
          teamId={selectedPlayer.teamId}
          teamColor={selectedPlayer.color}
          allPlayers={selectedPlayer.teamId === match.homeTeam.teamId ? (match.homePlayers ?? []) : (match.awayPlayers ?? [])}
          onAction={handleAction}
          onCancel={() => setSelectedPlayer(null)}
        />
      )}

      {/* Player Rosters — tap to select */}
      {match.status === 'Active' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div className="bg-slate-800 rounded-lg p-3">
            <PlayerRosterPanel
              players={match.homePlayers ?? []}
              teamName={match.homeTeam.teamName}
              selectedPlayerId={selectedPlayer?.teamId === match.homeTeam.teamId ? selectedPlayer.player.id : null}
              onSelectPlayer={(p) => handleSelectPlayer(p, match.homeTeam.teamId, 'blue')}
              color="blue"
            />
          </div>
          <div className="bg-slate-800 rounded-lg p-3">
            <PlayerRosterPanel
              players={match.awayPlayers ?? []}
              teamName={match.awayTeam.teamName}
              selectedPlayerId={selectedPlayer?.teamId === match.awayTeam.teamId ? selectedPlayer.player.id : null}
              onSelectPlayer={(p) => handleSelectPlayer(p, match.awayTeam.teamId, 'orange')}
              color="orange"
            />
          </div>
        </div>
      )}

      {/* Recent events */}
      <div className="bg-slate-800 rounded-lg p-4">
        <h3 className="text-white font-semibold mb-3">Recent Events</h3>
        {recentEvents.length === 0 ? (
          <p className="text-slate-400 text-sm">No events yet.</p>
        ) : (
          <ul className="space-y-2">
            {recentEvents.map((event) => {
              const isHome = event.teamId === match.homeTeam.teamId
              const teamName = isHome ? match.homeTeam.teamName : match.awayTeam.teamName
              return (
                <li key={event.id} className="flex items-center gap-2 text-sm">
                  <span className="text-slate-400 font-mono w-16 text-xs">
                    Q{event.periodNumber} {Math.floor(event.periodTimestamp / 60)}:{String(event.periodTimestamp % 60).padStart(2, '0')}
                  </span>
                  <span className={`font-medium ${isHome ? 'text-blue-400' : 'text-orange-400'}`}>
                    {teamName}
                  </span>
                  <span className="text-white">{event.type}</span>
                  {event.points && (
                    <span className="text-green-400">+{event.points}</span>
                  )}
                  <span className="text-slate-300">{resolvePlayerName(event.playerId)}</span>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
