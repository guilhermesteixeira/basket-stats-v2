import { useParams, useNavigate, Link } from 'react-router-dom'
import { useMatch } from '../hooks/useMatch'
import { Spinner } from '../components/ui/Spinner'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  BarChart,
  Bar,
  ResponsiveContainer,
} from 'recharts'
import type { MatchEvent } from '../types'

export function MatchStatsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: match, isLoading } = useMatch(id ?? '')

  if (!id) { navigate('/'); return null }

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

  // Build period score table
  const periodScores: Record<number, { home: number; away: number }> = {}
  for (const event of match.events) {
    if (!periodScores[event.periodNumber]) {
      periodScores[event.periodNumber] = { home: 0, away: 0 }
    }
    if (event.type === 'Score' && event.details.points) {
      const isHome = event.teamId === match.homeTeam.teamId
      if (isHome) periodScores[event.periodNumber].home += event.details.points
      else periodScores[event.periodNumber].away += event.details.points
    }
    if (event.type === 'FreeThrow' && event.details.made) {
      const isHome = event.teamId === match.homeTeam.teamId
      if (isHome) periodScores[event.periodNumber].home += 1
      else periodScores[event.periodNumber].away += 1
    }
  }

  // Build points-over-time data
  const timelineEvents = [...match.events]
    .filter((e) => e.type === 'Score' || (e.type === 'FreeThrow' && e.details.made))
    .sort((a, b) => {
      const aTime = (a.periodNumber - 1) * 600 + a.periodTimestamp
      const bTime = (b.periodNumber - 1) * 600 + b.periodTimestamp
      return aTime - bTime
    })

  let homeRunning = 0
  let awayRunning = 0
  const timelineData: Array<{ time: number; home: number; away: number }> = [{ time: 0, home: 0, away: 0 }]

  for (const event of timelineEvents) {
    const time = (event.periodNumber - 1) * 600 + event.periodTimestamp
    const pts = event.type === 'Score' ? (event.details.points ?? 2) : 1
    if (event.teamId === match.homeTeam.teamId) homeRunning += pts
    else awayRunning += pts
    timelineData.push({ time, home: homeRunning, away: awayRunning })
  }

  // Build event type breakdown
  const eventTypes = ['Score', 'MissedShot', 'FreeThrow', 'Foul', 'Substitution']
  const breakdownData = eventTypes.map((type) => {
    const homeCount = match.events.filter(
      (e: MatchEvent) => e.type === type && e.teamId === match.homeTeam.teamId,
    ).length
    const awayCount = match.events.filter(
      (e: MatchEvent) => e.type === type && e.teamId === match.awayTeam.teamId,
    ).length
    return { type, [match.homeTeam.teamName]: homeCount, [match.awayTeam.teamName]: awayCount }
  })

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">
            {match.homeTeam.teamName} vs {match.awayTeam.teamName}
          </h1>
          <div className="flex items-center gap-2 mt-1">
            <Badge status={match.status} />
            <span className="text-slate-400 text-sm">
              {match.homeScore} – {match.awayScore}
            </span>
          </div>
        </div>
        <Link to={`/matches/${id}/live`}>
          <Button variant="secondary">← Live</Button>
        </Link>
      </div>

      {/* Period score table */}
      <div className="bg-slate-800 rounded-lg p-4">
        <h2 className="text-white font-semibold mb-3">Score by Period</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-slate-400 border-b border-slate-700">
                <th className="text-left py-2 pr-4">Team</th>
                {[1, 2, 3, 4].map((p) => (
                  <th key={p} className="text-center py-2 px-3">Q{p}</th>
                ))}
                <th className="text-center py-2 px-3 font-bold">Total</th>
              </tr>
            </thead>
            <tbody>
              <tr className="text-white border-b border-slate-700">
                <td className="py-2 pr-4 font-medium text-blue-400">{match.homeTeam.teamName}</td>
                {[1, 2, 3, 4].map((p) => (
                  <td key={p} className="text-center py-2 px-3">{periodScores[p]?.home ?? 0}</td>
                ))}
                <td className="text-center py-2 px-3 font-bold">{match.homeScore}</td>
              </tr>
              <tr className="text-white">
                <td className="py-2 pr-4 font-medium text-orange-400">{match.awayTeam.teamName}</td>
                {[1, 2, 3, 4].map((p) => (
                  <td key={p} className="text-center py-2 px-3">{periodScores[p]?.away ?? 0}</td>
                ))}
                <td className="text-center py-2 px-3 font-bold">{match.awayScore}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      {/* Points over time */}
      <div className="bg-slate-800 rounded-lg p-4">
        <h2 className="text-white font-semibold mb-3">Points Over Time</h2>
        <ResponsiveContainer width="100%" height={260}>
          <LineChart data={timelineData}>
            <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
            <XAxis
              dataKey="time"
              stroke="#94a3b8"
              tickFormatter={(v: number) => `${Math.floor(v / 60)}'`}
              label={{ value: 'Time (min)', position: 'insideBottom', offset: -2, fill: '#94a3b8', fontSize: 11 }}
            />
            <YAxis stroke="#94a3b8" />
            <Tooltip
              contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #475569', color: '#f1f5f9' }}
              labelFormatter={(v) => `${Math.floor(Number(v) / 60)}m ${Number(v) % 60}s`}
            />
            <Legend />
            <Line
              type="stepAfter"
              dataKey="home"
              name={match.homeTeam.teamName}
              stroke="#3b82f6"
              dot={false}
              strokeWidth={2}
            />
            <Line
              type="stepAfter"
              dataKey="away"
              name={match.awayTeam.teamName}
              stroke="#f97316"
              dot={false}
              strokeWidth={2}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>

      {/* Event breakdown */}
      <div className="bg-slate-800 rounded-lg p-4">
        <h2 className="text-white font-semibold mb-3">Event Breakdown by Team</h2>
        <ResponsiveContainer width="100%" height={220}>
          <BarChart data={breakdownData}>
            <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
            <XAxis dataKey="type" stroke="#94a3b8" tick={{ fontSize: 11 }} />
            <YAxis stroke="#94a3b8" allowDecimals={false} />
            <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #475569', color: '#f1f5f9' }} />
            <Legend />
            <Bar dataKey={match.homeTeam.teamName} fill="#3b82f6" radius={[3, 3, 0, 0]} />
            <Bar dataKey={match.awayTeam.teamName} fill="#f97316" radius={[3, 3, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
