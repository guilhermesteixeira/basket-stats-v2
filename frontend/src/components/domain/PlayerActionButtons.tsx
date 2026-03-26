import { useState } from 'react'
import type { Player, Coordinates } from '../../types'
import { CourtMap } from './CourtMap'

type ActionType =
  | { kind: 'score'; points: 2 | 3 }
  | { kind: 'missed' }
  | { kind: 'turnover' }
  | { kind: 'foul'; foulType: string; flagrant: boolean }
  | { kind: 'subOut' }

interface PlayerActionButtonsProps {
  player: Player
  teamId: string
  teamColor: 'blue' | 'orange'
  allPlayers: Player[]
  onAction: (action: {
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
  }) => void
  onCancel: () => void
}

export function PlayerActionButtons({
  player,
  teamId,
  teamColor,
  allPlayers,
  onAction,
  onCancel,
}: PlayerActionButtonsProps) {
  const [pendingAction, setPendingAction] = useState<ActionType | null>(null)
  const [coords, setCoords] = useState<Coordinates | undefined>()
  const [playerFouledId, setPlayerFouledId] = useState('')
  const [subOutPlayerId, setSubOutPlayerId] = useState('')

  const borderColor = teamColor === 'blue' ? 'border-blue-500' : 'border-orange-500'
  const accentColor = teamColor === 'blue' ? 'text-blue-400' : 'text-orange-400'

  const handleQuickAction = (action: ActionType) => {
    if (action.kind === 'score' || action.kind === 'missed') {
      setPendingAction(action)
      return
    }
    if (action.kind === 'foul') {
      setPendingAction(action)
      return
    }
    if (action.kind === 'subOut') {
      setPendingAction(action)
      return
    }
    // Turnover — submit directly
    if (action.kind === 'turnover') {
      onAction({
        type: 'Turnover',
        teamId,
        playerId: player.id,
      })
    }
  }

  const confirmShotAction = () => {
    if (!coords || !pendingAction) return
    if (pendingAction.kind === 'score') {
      onAction({
        type: 'Score',
        teamId,
        playerId: player.id,
        points: pendingAction.points,
        coordinatesX: coords.x,
        coordinatesY: coords.y,
      })
    } else if (pendingAction.kind === 'missed') {
      onAction({
        type: 'MissedShot',
        teamId,
        playerId: player.id,
        coordinatesX: coords.x,
        coordinatesY: coords.y,
      })
    }
  }

  const confirmFoul = () => {
    if (!pendingAction || pendingAction.kind !== 'foul') return
    onAction({
      type: 'Foul',
      teamId,
      playerId: player.id,
      foulType: pendingAction.foulType,
      flagrant: pendingAction.flagrant,
      playerFouledId: playerFouledId || undefined,
    })
  }

  const confirmSub = () => {
    if (!subOutPlayerId) return
    onAction({
      type: 'Substitution',
      teamId,
      playerId: player.id,
      playerOutId: subOutPlayerId,
    })
  }

  // Show court map for shot events
  if (pendingAction && (pendingAction.kind === 'score' || pendingAction.kind === 'missed')) {
    return (
      <div className={`bg-slate-800 rounded-lg p-3 border ${borderColor} space-y-3`}>
        <div className="flex items-center justify-between">
          <span className="text-white text-sm font-medium">
            #{player.number} {player.name} — {pendingAction.kind === 'score' ? `${pendingAction.points}pt Shot` : 'Missed Shot'}
          </span>
          <button onClick={() => setPendingAction(null)} className="text-slate-400 hover:text-white text-xs">
            ← Back
          </button>
        </div>
        <CourtMap onSelect={setCoords} selectedCoords={coords} />
        <button
          onClick={confirmShotAction}
          disabled={!coords}
          className={`w-full py-2 rounded text-sm font-medium transition-colors ${
            coords ? 'bg-green-600 hover:bg-green-500 text-white' : 'bg-slate-700 text-slate-500 cursor-not-allowed'
          }`}
        >
          {coords ? '✓ Confirm' : 'Select shot location'}
        </button>
      </div>
    )
  }

  // Show foul details
  if (pendingAction && pendingAction.kind === 'foul') {
    return (
      <div className={`bg-slate-800 rounded-lg p-3 border ${borderColor} space-y-3`}>
        <div className="flex items-center justify-between">
          <span className="text-white text-sm font-medium">
            #{player.number} {player.name} — Foul
          </span>
          <button onClick={() => setPendingAction(null)} className="text-slate-400 hover:text-white text-xs">
            ← Back
          </button>
        </div>
        <div>
          <label className="block text-xs text-slate-400 mb-1">Player Fouled (optional)</label>
          <input
            type="text"
            value={playerFouledId}
            onChange={e => setPlayerFouledId(e.target.value)}
            placeholder="Player name"
            className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
          />
        </div>
        <button
          onClick={confirmFoul}
          className="w-full py-2 rounded text-sm font-medium bg-red-600 hover:bg-red-500 text-white"
        >
          ✓ Confirm Foul
        </button>
      </div>
    )
  }

  // Show sub picker
  if (pendingAction && pendingAction.kind === 'subOut') {
    const otherPlayers = allPlayers.filter(p => p.id !== player.id).sort((a, b) => a.number - b.number)
    return (
      <div className={`bg-slate-800 rounded-lg p-3 border ${borderColor} space-y-3`}>
        <div className="flex items-center justify-between">
          <span className="text-white text-sm font-medium">
            #{player.number} {player.name} enters — who leaves?
          </span>
          <button onClick={() => setPendingAction(null)} className="text-slate-400 hover:text-white text-xs">
            ← Back
          </button>
        </div>
        <div className="grid grid-cols-2 gap-1">
          {otherPlayers.map(p => (
            <button
              key={p.id}
              onClick={() => setSubOutPlayerId(p.id)}
              className={`px-2 py-1.5 rounded text-sm text-left ${
                subOutPlayerId === p.id ? 'bg-yellow-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600'
              }`}
            >
              #{p.number} {p.name}
            </button>
          ))}
        </div>
        <button
          onClick={confirmSub}
          disabled={!subOutPlayerId}
          className={`w-full py-2 rounded text-sm font-medium transition-colors ${
            subOutPlayerId ? 'bg-yellow-600 hover:bg-yellow-500 text-white' : 'bg-slate-700 text-slate-500 cursor-not-allowed'
          }`}
        >
          ✓ Confirm Substitution
        </button>
      </div>
    )
  }

  // Main action buttons
  return (
    <div className={`bg-slate-800 rounded-lg p-3 border ${borderColor} space-y-2`}>
      <div className="flex items-center justify-between mb-1">
        <span className={`text-sm font-bold ${accentColor}`}>
          #{player.number} {player.name}
        </span>
        <button onClick={onCancel} className="text-slate-400 hover:text-white text-xs">✕ Cancel</button>
      </div>

      <div className="grid grid-cols-3 gap-1.5">
        <button
          onClick={() => handleQuickAction({ kind: 'score', points: 2 })}
          className="bg-green-700 hover:bg-green-600 text-white py-2.5 rounded text-sm font-bold"
        >
          2 PTS
        </button>
        <button
          onClick={() => handleQuickAction({ kind: 'score', points: 3 })}
          className="bg-green-800 hover:bg-green-700 text-white py-2.5 rounded text-sm font-bold"
        >
          3 PTS
        </button>
        <button
          onClick={() => handleQuickAction({ kind: 'missed' })}
          className="bg-slate-600 hover:bg-slate-500 text-white py-2.5 rounded text-sm font-bold"
        >
          MISS
        </button>
        <button
          onClick={() => handleQuickAction({ kind: 'turnover' })}
          className="bg-purple-700 hover:bg-purple-600 text-white py-2.5 rounded text-sm font-bold"
        >
          TO
        </button>
        <button
          onClick={() => handleQuickAction({ kind: 'foul', foulType: 'Personal', flagrant: false })}
          className="bg-red-700 hover:bg-red-600 text-white py-2.5 rounded text-sm font-bold"
        >
          FOUL
        </button>
      </div>

      <button
        onClick={() => handleQuickAction({ kind: 'subOut' })}
        className="w-full bg-yellow-700 hover:bg-yellow-600 text-white py-2 rounded text-sm font-bold"
      >
        ↔ SUBSTITUTION
      </button>
    </div>
  )
}
