import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { createTeam } from '../../api/teams'
import { Button } from '../ui/Button'

export function CreateTeamModal() {
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) return
    setSubmitting(true)
    setError(null)
    try {
      await createTeam({ name: name.trim() })
      await queryClient.invalidateQueries({ queryKey: ['teams'] })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create team')
      setSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/95">
      <div className="bg-slate-800 border border-slate-700 rounded-xl p-8 w-full max-w-md shadow-2xl">
        <h1 className="text-2xl font-bold text-white mb-2">Welcome!</h1>
        <p className="text-slate-400 mb-6">Create your team to get started.</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm text-slate-300 mb-1">Team name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Los Angeles Lakers"
              required
              className="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          {error && <p className="text-red-400 text-sm">{error}</p>}
          <Button type="submit" loading={submitting} className="w-full">
            Create Team
          </Button>
        </form>
      </div>
    </div>
  )
}
