import { useParams, useNavigate, Link } from 'react-router-dom'
import { useMatch } from '../hooks/useMatch'
import { useAuthStore } from '../auth/AuthProvider'
import { useNetworkStatus } from '../hooks/useNetworkStatus'
import { useSyncQueue } from '../hooks/useSyncQueue'
import { startMatch, finishMatch } from '../api/matches'
import { useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { OfflineBanner } from '../components/offline/OfflineBanner'
import { EventForm } from '../components/domain/EventForm'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Spinner } from '../components/ui/Spinner'

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

  const recentEvents = [...match.events].reverse().slice(0, 10)

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-4">
      {!online && <OfflineBanner pendingCount={pendingCount} />}

      {/* Match header */}
      <div className="bg-slate-800 rounded-lg p-4">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <Badge status={match.status} />
            {isSyncing && (
              <span className="text-xs text-blue-400 flex items-center gap-1">
                <Spinner size="sm" /> Syncing...
              </span>
            )}
          </div>
          <div className="flex gap-2">
            <Link to={`/matches/${id}/stats`}>
              <Button variant="secondary" className="text-xs">Stats</Button>
            </Link>
            {isAdmin && match.status === 'Scheduled' && (
              <Button onClick={handleStart} loading={starting} className="text-xs">Start Match</Button>
            )}
            {isAdmin && match.status === 'Active' && (
              <Button variant="danger" onClick={handleFinish} loading={finishing} className="text-xs">Finish Match</Button>
            )}
          </div>
        </div>

        <div className="flex items-center justify-between text-center">
          <div className="flex-1">
            <p className="text-white font-bold text-lg">{match.homeTeam.teamName}</p>
            <p className="text-5xl font-bold text-white mt-1">{match.homeScore}</p>
          </div>
          <div className="text-slate-500 px-4">
            <span className="text-sm">Q{match.periods.length > 0 ? match.periods[match.periods.length - 1].number : 1}</span>
          </div>
          <div className="flex-1">
            <p className="text-white font-bold text-lg">{match.awayTeam.teamName}</p>
            <p className="text-5xl font-bold text-white mt-1">{match.awayScore}</p>
          </div>
        </div>
      </div>

      {/* Event forms — only for active matches */}
      {match.status === 'Active' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="bg-slate-800 rounded-lg p-4">
            <h3 className="text-white font-semibold mb-3">
              {match.homeTeam.teamName} — Event
            </h3>
            <EventForm
              matchId={id}
              teamId={match.homeTeam.teamId}
              onSuccess={() => queryClient.invalidateQueries({ queryKey: ['matches', id] })}
            />
          </div>
          <div className="bg-slate-800 rounded-lg p-4">
            <h3 className="text-white font-semibold mb-3">
              {match.awayTeam.teamName} — Event
            </h3>
            <EventForm
              matchId={id}
              teamId={match.awayTeam.teamId}
              onSuccess={() => queryClient.invalidateQueries({ queryKey: ['matches', id] })}
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
                  {event.playerId && (
                    <span className="text-slate-300">({event.playerId})</span>
                  )}
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
