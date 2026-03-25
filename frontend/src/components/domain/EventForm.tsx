import { useState } from 'react'
import type { EventType, Coordinates, EventDetails } from '../../types'
import { useAddEvent } from '../../hooks/useAddEvent'
import { useNetworkStatus } from '../../hooks/useNetworkStatus'
import { Button } from '../ui/Button'
import { CourtMap } from './CourtMap'

interface EventFormProps {
  matchId: string
  teamId: string
  onSuccess: () => void
}

const EVENT_TYPES: EventType[] = ['Score', 'MissedShot', 'FreeThrow', 'Foul', 'Substitution']

export function EventForm({ matchId, teamId, onSuccess }: EventFormProps) {
  const { online } = useNetworkStatus()
  const { mutate, isPending, isSuccess, reset } = useAddEvent(matchId)

  const [eventType, setEventType] = useState<EventType>('Score')
  const [coords, setCoords] = useState<Coordinates | undefined>()
  const [details, setDetails] = useState<EventDetails>({})
  const [periodNumber, setPeriodNumber] = useState(1)
  const [periodTimestamp, setPeriodTimestamp] = useState(0)

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    const finalDetails: EventDetails = { ...details }
    if (eventType === 'Score' || eventType === 'MissedShot') {
      finalDetails.coordinates = coords
    }

    mutate(
      { type: eventType, teamId, periodNumber, periodTimestamp, details: finalDetails },
      {
        onSuccess: () => {
          setDetails({})
          setCoords(undefined)
          onSuccess()
          setTimeout(reset, 2000)
        },
      },
    )
  }

  const needsCoords = eventType === 'Score' || eventType === 'MissedShot'

  return (
    <form onSubmit={handleSubmit} className="space-y-3">
      {!online && (
        <p className="text-xs text-yellow-400 bg-yellow-900/30 rounded px-2 py-1">
          Offline — event will be queued
        </p>
      )}

      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-xs text-slate-400 mb-1">Period</label>
          <select
            value={periodNumber}
            onChange={(e) => setPeriodNumber(Number(e.target.value))}
            className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
          >
            {[1, 2, 3, 4].map((p) => (
              <option key={p} value={p}>Q{p}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs text-slate-400 mb-1">Time (sec)</label>
          <input
            type="number"
            min={0}
            max={600}
            value={periodTimestamp}
            onChange={(e) => setPeriodTimestamp(Number(e.target.value))}
            className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
          />
        </div>
      </div>

      <div>
        <label className="block text-xs text-slate-400 mb-1">Event Type</label>
        <select
          value={eventType}
          onChange={(e) => { setEventType(e.target.value as EventType); setDetails({}); setCoords(undefined) }}
          className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
        >
          {EVENT_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>

      {eventType === 'Score' && (
        <>
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="block text-xs text-slate-400 mb-1">Points</label>
              <select
                value={details.points ?? 2}
                onChange={(e) => setDetails((d) => ({ ...d, points: Number(e.target.value) }))}
                className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
              >
                <option value={2}>2 pts</option>
                <option value={3}>3 pts</option>
              </select>
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Player</label>
              <input
                type="text"
                value={details.playerName ?? ''}
                onChange={(e) => setDetails((d) => ({ ...d, playerName: e.target.value }))}
                placeholder="Player name"
                className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
              />
            </div>
          </div>
        </>
      )}

      {eventType === 'MissedShot' && (
        <div>
          <label className="block text-xs text-slate-400 mb-1">Player</label>
          <input
            type="text"
            value={details.playerName ?? ''}
            onChange={(e) => setDetails((d) => ({ ...d, playerName: e.target.value }))}
            placeholder="Player name"
            className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
          />
        </div>
      )}

      {eventType === 'FreeThrow' && (
        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="block text-xs text-slate-400 mb-1">Result</label>
            <select
              value={details.made === true ? 'made' : 'missed'}
              onChange={(e) => setDetails((d) => ({ ...d, made: e.target.value === 'made' }))}
              className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
            >
              <option value="made">Made</option>
              <option value="missed">Missed</option>
            </select>
          </div>
          <div>
            <label className="block text-xs text-slate-400 mb-1">Foul Type</label>
            <select
              value={details.foulType ?? 'personal'}
              onChange={(e) => setDetails((d) => ({ ...d, foulType: e.target.value as EventDetails['foulType'] }))}
              className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
            >
              <option value="personal">Personal</option>
              <option value="technical">Technical</option>
              <option value="flagrant">Flagrant</option>
            </select>
          </div>
          <div className="col-span-2">
            <label className="block text-xs text-slate-400 mb-1">Player</label>
            <input
              type="text"
              value={details.playerName ?? ''}
              onChange={(e) => setDetails((d) => ({ ...d, playerName: e.target.value }))}
              placeholder="Player name"
              className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
            />
          </div>
        </div>
      )}

      {eventType === 'Foul' && (
        <div className="space-y-2">
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="block text-xs text-slate-400 mb-1">Foul Type</label>
              <select
                value={details.foulType ?? 'personal'}
                onChange={(e) => setDetails((d) => ({ ...d, foulType: e.target.value as EventDetails['foulType'] }))}
                className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
              >
                <option value="personal">Personal</option>
                <option value="technical">Technical</option>
                <option value="flagrant">Flagrant</option>
              </select>
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Player Fouled</label>
              <input
                type="text"
                value={details.playerFouled ?? ''}
                onChange={(e) => setDetails((d) => ({ ...d, playerFouled: e.target.value }))}
                placeholder="Name"
                className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
              />
            </div>
          </div>
          <label className="flex items-center gap-2 text-sm text-white cursor-pointer">
            <input
              type="checkbox"
              checked={details.flagrant ?? false}
              onChange={(e) => setDetails((d) => ({ ...d, flagrant: e.target.checked }))}
              className="rounded"
            />
            Flagrant foul
          </label>
        </div>
      )}

      {eventType === 'Substitution' && (
        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="block text-xs text-slate-400 mb-1">Player In</label>
            <input
              type="text"
              value={details.playerIn ?? ''}
              onChange={(e) => setDetails((d) => ({ ...d, playerIn: e.target.value }))}
              placeholder="Entering"
              className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
            />
          </div>
          <div>
            <label className="block text-xs text-slate-400 mb-1">Player Out</label>
            <input
              type="text"
              value={details.playerOut ?? ''}
              onChange={(e) => setDetails((d) => ({ ...d, playerOut: e.target.value }))}
              placeholder="Leaving"
              className="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-white text-sm"
            />
          </div>
        </div>
      )}

      {needsCoords && (
        <CourtMap onSelect={setCoords} selectedCoords={coords} />
      )}

      {isSuccess && (
        <p className="text-xs text-green-400 text-center">
          {online ? '✓ Event added' : '✓ Event queued (offline)'}
        </p>
      )}

      <Button type="submit" loading={isPending} className="w-full">
        Add Event
      </Button>
    </form>
  )
}
