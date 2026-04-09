import { useState, useEffect, useRef, useCallback } from 'react'

const PERIOD_DURATION = 600 // 10 minutes in seconds (FIBA)

interface GameClockProps {
  matchStatus: 'Scheduled' | 'Active' | 'Finished'
  periods: { number: number; startTime?: string; endTime?: string }[]
  onTimeUpdate?: (periodNumber: number, periodTimestamp: number) => void
}

export function GameClock({ matchStatus, periods, onTimeUpdate }: GameClockProps) {
  const [currentPeriod, setCurrentPeriod] = useState(1)
  const [elapsed, setElapsed] = useState(0) // seconds elapsed in current period
  const [running, setRunning] = useState(false)
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const onTimeUpdateRef = useRef(onTimeUpdate)
  onTimeUpdateRef.current = onTimeUpdate

  // Derive initial period from match data
  useEffect(() => {
    if (periods.length > 0) {
      const lastActive = [...periods].reverse().find(p => p.startTime && !p.endTime)
      const lastEnded = [...periods].reverse().find(p => p.endTime)
      if (lastActive) {
        setCurrentPeriod(lastActive.number)
      } else if (lastEnded) {
        setCurrentPeriod(Math.min(lastEnded.number + 1, 4))
      }
    }
  }, [periods])

  const tick = useCallback(() => {
    setElapsed(prev => {
      const next = prev + 1
      if (next >= PERIOD_DURATION) {
        setRunning(false)
        return PERIOD_DURATION
      }
      return next
    })
  }, [])

  useEffect(() => {
    if (running) {
      intervalRef.current = setInterval(tick, 1000)
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [running, tick])

  // Broadcast time
  useEffect(() => {
    onTimeUpdateRef.current?.(currentPeriod, elapsed)
  }, [currentPeriod, elapsed])

  const remaining = PERIOD_DURATION - elapsed
  const minutes = Math.floor(remaining / 60)
  const seconds = remaining % 60

  const handleStartPause = () => {
    if (matchStatus !== 'Active') return
    setRunning(r => !r)
  }

  const handleNextPeriod = () => {
    if (currentPeriod >= 4) return
    setRunning(false)
    setElapsed(0)
    setCurrentPeriod(p => p + 1)
  }

  const handleReset = () => {
    setRunning(false)
    setElapsed(0)
  }

  const isFinished = matchStatus === 'Finished'
  const isScheduled = matchStatus === 'Scheduled'

  return (
    <div className="flex flex-col items-center gap-2">
      {/* Period indicator */}
      <div className="flex gap-1">
        {[1, 2, 3, 4].map(p => (
          <button
            key={p}
            onClick={() => { if (!running) { setCurrentPeriod(p); setElapsed(0) } }}
            className={`px-3 py-1 rounded text-xs font-bold transition-colors ${
              p === currentPeriod
                ? 'bg-blue-600 text-white'
                : p < currentPeriod
                  ? 'bg-slate-600 text-slate-300'
                  : 'bg-slate-700 text-slate-500'
            } ${running ? 'cursor-default' : 'cursor-pointer hover:bg-blue-500'}`}
            disabled={running}
          >
            Q{p}
          </button>
        ))}
      </div>

      {/* Clock display */}
      <div className={`font-mono text-4xl font-bold tabular-nums ${
        running ? 'text-green-400' : remaining === 0 ? 'text-red-400' : 'text-white'
      }`}>
        {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}
      </div>

      {/* Controls */}
      {!isFinished && !isScheduled && (
        <div className="flex gap-2">
          <button
            onClick={handleStartPause}
            className={`px-4 py-1.5 rounded text-sm font-medium transition-colors ${
              running
                ? 'bg-yellow-600 hover:bg-yellow-500 text-white'
                : 'bg-green-600 hover:bg-green-500 text-white'
            }`}
          >
            {running ? '⏸ Pause' : '▶ Start'}
          </button>
          <button
            onClick={handleReset}
            className="px-3 py-1.5 rounded text-sm font-medium bg-slate-700 hover:bg-slate-600 text-slate-300"
          >
            ↺ Reset
          </button>
          {elapsed >= PERIOD_DURATION - 1 && currentPeriod < 4 && (
            <button
              onClick={handleNextPeriod}
              className="px-4 py-1.5 rounded text-sm font-medium bg-blue-600 hover:bg-blue-500 text-white"
            >
              Next Q{currentPeriod + 1} →
            </button>
          )}
        </div>
      )}
    </div>
  )
}
