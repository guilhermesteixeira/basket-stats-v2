import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMatches } from '../hooks/useMatches'
import { useTeams } from '../hooks/useTeams'
import { useAuthStore } from '../auth/AuthProvider'
import { createMatch } from '../api/matches'
import { MatchCard } from '../components/domain/MatchCard'
import { Button } from '../components/ui/Button'
import { Spinner } from '../components/ui/Spinner'

export function MatchListPage() {
  const navigate = useNavigate()
  const { data: matches, isLoading, error } = useMatches()
  const { data: teams } = useTeams()
  const { userRoles } = useAuthStore()
  const canCreate = userRoles.includes('match-creator') || userRoles.includes('admin')

  const [showForm, setShowForm] = useState(false)
  const [homeTeamId, setHomeTeamId] = useState('')
  const [awayTeamId, setAwayTeamId] = useState('')
  const [creating, setCreating] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!homeTeamId || !awayTeamId) return
    setCreating(true)
    setCreateError(null)
    try {
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
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-white">Matches</h1>
        {canCreate && (
          <Button onClick={() => setShowForm((v) => !v)}>
            {showForm ? 'Cancel' : '+ New Match'}
          </Button>
        )}
      </div>

      {showForm && (
        <div className="bg-slate-800 rounded-lg p-4 mb-6 border border-slate-700">
          <h2 className="text-white font-semibold mb-4">Create New Match</h2>
          <form onSubmit={handleCreate} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-slate-400 mb-1">Home Team</label>
                <select
                  value={homeTeamId}
                  onChange={(e) => setHomeTeamId(e.target.value)}
                  required
                  className="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white text-sm"
                >
                  <option value="">Select team...</option>
                  {teams?.map((t) => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs text-slate-400 mb-1">Away Team</label>
                <select
                  value={awayTeamId}
                  onChange={(e) => setAwayTeamId(e.target.value)}
                  required
                  className="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white text-sm"
                >
                  <option value="">Select team...</option>
                  {teams?.map((t) => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </select>
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
