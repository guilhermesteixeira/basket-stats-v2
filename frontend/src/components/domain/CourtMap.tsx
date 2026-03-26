import type { Coordinates } from '../../types'

interface CourtMapProps {
  onSelect: (coords: Coordinates) => void
  selectedCoords?: Coordinates
}

export function CourtMap({ onSelect, selectedCoords }: CourtMapProps) {
  const handleClick = (e: React.MouseEvent<SVGSVGElement>) => {
    const svg = e.currentTarget
    const rect = svg.getBoundingClientRect()
    const x = Math.round(((e.clientX - rect.left) / rect.width) * 100)
    const y = Math.round(((e.clientY - rect.top) / rect.height) * 100)
    onSelect({ x, y })
  }

  return (
    <div className="relative w-full max-w-xs mx-auto">
      <p className="text-xs text-slate-400 mb-1 text-center">Click to select shot location</p>
      <svg
        viewBox="0 0 200 180"
        className="w-full border border-slate-600 rounded cursor-crosshair bg-amber-900"
        onClick={handleClick}
        aria-label="Basketball court - click to select shot location"
      >
        {/* Court outline */}
        <rect x="5" y="5" width="190" height="170" fill="none" stroke="#c97c3a" strokeWidth="2" />

        {/* Paint / key */}
        <rect x="75" y="100" width="50" height="75" fill="none" stroke="white" strokeWidth="1.5" />

        {/* Free throw circle */}
        <ellipse cx="100" cy="100" rx="25" ry="15" fill="none" stroke="white" strokeWidth="1.5" />

        {/* Free throw line */}
        <line x1="75" y1="100" x2="125" y2="100" stroke="white" strokeWidth="1.5" />

        {/* Basket */}
        <circle cx="100" cy="168" r="4" fill="none" stroke="orange" strokeWidth="2" />
        <line x1="96" y1="172" x2="104" y2="172" stroke="orange" strokeWidth="2" />

        {/* Three-point arc */}
        <path
          d="M 20 175 L 20 120 A 80 80 0 0 1 180 120 L 180 175"
          fill="none"
          stroke="white"
          strokeWidth="1.5"
        />

        {/* Center court mark */}
        <circle cx="100" cy="30" r="20" fill="none" stroke="white" strokeWidth="1" />
        <circle cx="100" cy="30" r="3" fill="white" />

        {/* Selected coordinates dot */}
        {selectedCoords && (
          <circle
            cx={(selectedCoords.x / 100) * 200}
            cy={(selectedCoords.y / 100) * 180}
            r="6"
            fill="blue"
            stroke="white"
            strokeWidth="1.5"
            className="pointer-events-none"
          />
        )}
      </svg>
      {selectedCoords && (
        <p className="text-xs text-slate-400 text-center mt-1">
          Selected: ({selectedCoords.x}, {selectedCoords.y})
        </p>
      )}
    </div>
  )
}
