import { useState } from 'react'
import type { PlayerInput } from '../../types'
import { Button } from '../ui/Button'

interface PlayerRosterFormProps {
  label: string
  players: PlayerInput[]
  onChange: (players: PlayerInput[]) => void
}

export function PlayerRosterForm({ label, players, onChange }: PlayerRosterFormProps) {
  const [name, setName] = useState('')
  const [number, setNumber] = useState('')

  const addPlayer = () => {
    const trimmedName = name.trim()
    const num = parseInt(number)
    if (!trimmedName || isNaN(num) || num < 0 || num > 99) return
    if (players.some(p => p.number === num)) return // no duplicate numbers
    onChange([...players, { name: trimmedName, number: num }])
    setName('')
    setNumber('')
  }

  const removePlayer = (index: number) => {
    onChange(players.filter((_, i) => i !== index))
  }

  const sorted = [...players].sort((a, b) => a.number - b.number)

  return (
    <div>
      <label className="block text-xs text-slate-400 mb-2">{label}</label>

      {sorted.length > 0 && (
        <div className="space-y-1 mb-2">
          {sorted.map((p, idx) => {
            const originalIdx = players.indexOf(p)
            return (
              <div key={idx} className="flex items-center gap-2 bg-slate-700/50 rounded px-2 py-1 text-sm">
                <span className="font-mono text-blue-400 font-bold w-8">#{p.number}</span>
                <span className="text-white flex-1">{p.name}</span>
                <button
                  type="button"
                  onClick={() => removePlayer(originalIdx)}
                  className="text-red-400 hover:text-red-300 text-xs"
                >
                  ✕
                </button>
              </div>
            )
          })}
        </div>
      )}

      <div className="flex gap-2">
        <input
          type="number"
          min={0}
          max={99}
          value={number}
          onChange={e => setNumber(e.target.value)}
          placeholder="#"
          className="w-16 bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm placeholder-slate-500"
          onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); addPlayer() } }}
        />
        <input
          type="text"
          value={name}
          onChange={e => setName(e.target.value)}
          placeholder="Player name"
          className="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm placeholder-slate-500"
          onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); addPlayer() } }}
        />
        <Button type="button" variant="secondary" onClick={addPlayer} className="text-xs">
          + Add
        </Button>
      </div>

      <p className="text-xs text-slate-500 mt-1">
        {players.length} player{players.length !== 1 ? 's' : ''} added
        {players.length < 5 && ' (min 5 recommended)'}
      </p>
    </div>
  )
}
