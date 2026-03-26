import { Spinner } from '../components/ui/Spinner'

export function LoginPage() {
  return (
    <div className="min-h-screen bg-slate-900 flex flex-col items-center justify-center gap-6">
      <h1 className="text-4xl font-bold text-white">🏀 Basket Stats</h1>
      <div className="flex items-center gap-3 text-slate-300">
        <Spinner size="md" />
        <span>Loading...</span>
      </div>
    </div>
  )
}
