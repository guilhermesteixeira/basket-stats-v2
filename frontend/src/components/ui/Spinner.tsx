type SpinnerSize = 'sm' | 'md' | 'lg'

interface SpinnerProps {
  size?: SpinnerSize
}

const sizeClasses: Record<SpinnerSize, string> = {
  sm: 'w-4 h-4 border-2',
  md: 'w-8 h-8 border-2',
  lg: 'w-12 h-12 border-4',
}

export function Spinner({ size = 'md' }: SpinnerProps) {
  return (
    <div
      className={`${sizeClasses[size]} rounded-full border-blue-600 border-t-transparent animate-spin`}
      role="status"
      aria-label="Loading"
    />
  )
}
