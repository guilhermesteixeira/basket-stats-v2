interface OfflineBannerProps {
  pendingCount?: number
}

export function OfflineBanner({ pendingCount = 0 }: OfflineBannerProps) {
  return (
    <div className="bg-yellow-500 text-yellow-900 px-4 py-2 text-sm font-medium" role="alert">
      <span>⚠️ You are offline. Events will be saved locally and synced when reconnected.</span>
      {pendingCount > 0 && (
        <span className="ml-2 bg-yellow-700 text-yellow-100 rounded-full px-2 py-0.5 text-xs">
          {pendingCount} pending
        </span>
      )}
    </div>
  )
}
