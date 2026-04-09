import type { Player } from '../../types'

interface PlayerRosterPanelProps {
  players: Player[]
  teamName: string
  selectedPlayerId: string | null
  onSelectPlayer: (player: Player) => void
  color: 'blue' | 'orange'
}

export function PlayerRosterPanel({
  players,
  teamName,
  selectedPlayerId,
  onSelectPlayer,
  color,
}: PlayerRosterPanelProps) {
  const sorted = [...players].sort((a, b) => a.number - b.number)
  const colorClasses = color === 'blue'
    ? { selected: 'bg-blue-600 ring-2 ring-blue-400', normal: 'bg-slate-700 hover:bg-blue-900/50', text: 'text-blue-400' }
    : { selected: 'bg-orange-600 ring-2 ring-orange-400', normal: 'bg-slate-700 hover:bg-orange-900/50', text: 'text-orange-400' }

  return (
    <div>
      <h3 className={`text-sm font-semibold mb-2 ${colorClasses.text}`}>{teamName}</h3>
      {sorted.length === 0 ? (
        <p className="text-slate-500 text-xs">No players registered</p>
      ) : (
        <div className="grid grid-cols-2 gap-1.5">
          {sorted.map(player => (
            <button
              key={player.id}
              onClick={() => onSelectPlayer(player)}
              className={`flex items-center gap-2 px-2 py-1.5 rounded text-left transition-all text-sm ${
                selectedPlayerId === player.id ? colorClasses.selected : colorClasses.normal
              }`}
            >
              <span className={`font-mono font-bold text-xs w-6 text-center ${
                selectedPlayerId === player.id ? 'text-white' : colorClasses.text
              }`}>
                #{player.number}
              </span>
              <span className={`truncate ${
                selectedPlayerId === player.id ? 'text-white font-medium' : 'text-slate-300'
              }`}>
                {player.name}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
