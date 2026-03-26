import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { useMatches } from '../hooks/useMatches'
import { useTeams } from '../hooks/useTeams'
import { useMyTeam } from '../hooks/useMyTeam'
import { useAuthStore } from '../auth/AuthProvider'
import { createMatch } from '../api/matches'
import { createTeam } from '../api/teams'
import { CreateTeamModal } from '../components/onboarding/CreateTeamModal'
import { MatchCard } from '../components/domain/MatchCard'
import { Button } from '../components/ui/Button'
import { Spinner } from '../components/ui/Spinner'

export function MatchListPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: matches, isLoading, error } = useMatches()
  const { data: teams } = useTeams()
  const myTeam = useMyTeam()
  const { userRoles } = useAuthStore()
  const canCreate = userRoles.includes('match-creator') || userRoles.includes('admin')

  const [showForm, setShowForm] = useState(false)
  const [opponentId, setOpponentId] = useState('')
  const [side, setSide] = useState<'home' | 'away'>('home')
  const [creating, setCreating] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)

  // Inline "create new team" state
  const [showNewTeam, setShowNewTeam] = useState(false)
  const [newTeamName, setNewTeamName] = useState('')
  const [creatingTeam, setCreatingTeam] = useState(false)
  const [newTeamError, setNewTeamError] = useState<string | null>(null)

  const opponentTeams = teams?.filter((t) => t.id !== myTeam?.id) ?? []

  const handleCreateTeam = async (e: React.FormEvent | React.MouseEvent) => {
    e.preventDefault()
    if (!newTeamName.trim()) return
    setCreatingTeam(true)
    setNewTeamError(null)
    try {
      const team = await createTeam({ name: newTeamName.trim() })
      await queryClient.invalidateQueries({ queryKey: ['teams'] })
      setOpponentId(team.id)
      setShowNewTeam(false)
      setNewTeamName('')
    } catch (err) {
      setNewTeamError(err instanceof Error ? err.message : 'Failed to create team')
    } finally {
      setCreatingTeam(false)
    }
  }

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!myTeam || !opponentId) return
    setCreating(true)
    setCreateError(null)
    try {
      const homeTeamId = side === 'home' ? myTeam.id : opponentId
      const awayTeamId = side === 'home' ? opponentId : myTeam.id
      const match = await createMatch({ homeTeamId, awayTeamId })
      navigate(`/matches/${match.id}/live`)
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : 'Failed to create match')
    } finally {
      setCreating(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <Spinner size="lg" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-20 text-red-400">
        Failed to load matches. Please try again.
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      {canCreate && !myTeam && <CreateTeamModal />}

      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-white">Matches</h1>
        {canCreate && myTeam && (
          <Button onClick={() => setShowForm((v) => !v)}>
            {showForm ? 'Cancel' : '+ New Match'}
          </Button>
        )}
      </div>

      {showForm && myTeam && (
        <div className="bg-slate-800 rounded-lg p-4 mb-6 border border-slate-700">
          <h2 className="text-white font-semibold mb-4">Create New Match</h2>
          <form onSubmit={handleCreate} className="space-y-4">
            {/* Your team (readonly) */}
            <div>
              <label className="block text-xs text-slate-400 mb-1">Your team</label>
              <div className="flex items-center gap-2 bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white text-sm">
                <span>{myTeam.name}</span>
                <span className="text-green-400">✓</span>
              </div>
            </div>

            {/* Opponent */}
            <div>
              <label className="block text-xs text-slate-400 mb-1">Opponent</label>
              <div className="flex gap-2">
                <select
                  value={opponentId}
                  onChange={(e) => setOpponentId(e.target.value)}
                  required
                  className="flex-1 bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white text-sm"
                >
                  <option value="">Select team...</option>
                  {opponentTeams.map((t) => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </select>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => setShowNewTeam((v) => !v)}
                >
                  {showNewTeam ? 'Cancel' : '+ Create new team'}
                </Button>
              </div>
            </div>

            {/* Inline create opponent team */}
            {showNewTeam && (
              <div className="flex gap-2 items-start pl-1">
                <div className="flex-1">
                  <input
                    type="text"
                    value={newTeamName}
                    onChange={(e) => setNewTeamName(e.target.value)}
                    placeholder="New team name"
                    className="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white text-sm placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); void handleCreateTeam(e as unknown as React.FormEvent) } }}
                  />
                  {newTeamError && <p className="text-red-400 text-xs mt-1">{newTeamError}</p>}
                </div>
                <Button
                  type="button"
                  loading={creatingTeam}
                  variant="secondary"
                  onClick={(e) => void handleCreateTeam(e as unknown as React.FormEvent)}
                >
                  Add
                </Button>
              </div>
            )}

            {/* Home / Away */}
            <div>
              <label className="block text-xs text-slate-400 mb-2">Playing</label>
              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-sm text-white cursor-pointer">
                  <input
                    type="radio"
                    name="side"
                    value="home"
                    checked={side === 'home'}
                    onChange={() => setSide('home')}
                    className="accent-blue-500"
                  />
                  Home
                </label>
                <label className="flex items-center gap-2 text-sm text-white cursor-pointer">
                  <input
                    type="radio"
                    name="side"
                    value="away"
                    checked={side === 'away'}
                    onChange={() => setSide('away')}
                    className="accent-blue-500"
                  />
                  Away
                </label>
              </div>
            </div>

            {createError && <p className="text-red-400 text-sm">{createError}</p>}
            <Button type="submit" loading={creating}>Create Match</Button>
          </form>
        </div>
      )}

      {matches && matches.length === 0 ? (
        <p className="text-center text-slate-400 py-20">No matches yet.</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {matches?.map((match) => (
            <MatchCard
              key={match.id}
              match={match}
              onClick={() => navigate(`/matches/${match.id}/live`)}
            />
          ))}
        </div>
      )}
    </div>
  )
}
