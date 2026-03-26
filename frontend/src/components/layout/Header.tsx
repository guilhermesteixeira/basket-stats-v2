import { Link } from 'react-router-dom'
import keycloak from '../../auth/keycloak'
import { useNetworkStatus } from '../../hooks/useNetworkStatus'
import { Button } from '../ui/Button'

export function Header() {
  const { online } = useNetworkStatus()
  const email = keycloak.idTokenParsed?.email as string | undefined

  return (
    <header className="bg-slate-900 border-b border-slate-700 px-4 py-3">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/" className="text-white font-bold text-lg hover:text-blue-400 transition-colors">🏀 Basket Stats</Link>
          <span
            className={`w-2.5 h-2.5 rounded-full ${online ? 'bg-green-400' : 'bg-red-500'}`}
            title={online ? 'Online' : 'Offline'}
          />
        </div>

        <div className="flex items-center gap-3">
          {email && <span className="text-slate-300 text-sm">{email}</span>}
          <Button
            variant="secondary"
            onClick={() => keycloak.logout()}
            className="text-xs"
          >
            Logout
          </Button>
        </div>
      </div>
    </header>
  )
}
