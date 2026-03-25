import type { MatchStatus } from '../../types'

interface BadgeProps {
  status: MatchStatus
}

const statusConfig: Record<MatchStatus, { label: string; classes: string }> = {
  Scheduled: { label: 'Scheduled', classes: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200' },
  Active: { label: 'Live', classes: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' },
  Finished: { label: 'Finished', classes: 'bg-slate-200 text-slate-700 dark:bg-slate-700 dark:text-slate-300' },
}

export function Badge({ status }: BadgeProps) {
  const { label, classes } = statusConfig[status]
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${classes}`}>
      {status === 'Active' && (
        <span className="w-1.5 h-1.5 rounded-full bg-green-500 mr-1.5 animate-pulse" />
      )}
      {label}
    </span>
  )
}
