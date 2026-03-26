import type { Match } from '../../types'
import { Badge } from '../ui/Badge'
import { Card } from '../ui/Card'

interface MatchCardProps {
  match: Match
  onClick: () => void
}

export function MatchCard({ match, onClick }: MatchCardProps) {
  const date = new Date(match.createdAt).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })

  return (
    <Card
      className="cursor-pointer hover:ring-2 hover:ring-blue-500 transition-all"
      onClick={onClick}
    >
      <div className="flex justify-between items-start mb-3">
        <Badge status={match.status} />
        <span className="text-xs text-slate-400">{date}</span>
      </div>

      <div className="flex items-center justify-between gap-4">
        <div className="flex-1 text-center">
          <p className="font-semibold text-white truncate">{match.homeTeam.teamName}</p>
          <p className="text-3xl font-bold text-white mt-1">{match.homeScore}</p>
        </div>

        <div className="text-slate-500 font-bold text-lg">vs</div>

        <div className="flex-1 text-center">
          <p className="font-semibold text-white truncate">{match.awayTeam.teamName}</p>
          <p className="text-3xl font-bold text-white mt-1">{match.awayScore}</p>
        </div>
      </div>
    </Card>
  )
}
