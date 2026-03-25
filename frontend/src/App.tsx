import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from './auth/AuthProvider'
import { Header } from './components/layout/Header'
import { MatchListPage } from './pages/MatchListPage'
import { MatchLivePage } from './pages/MatchLivePage'
import { MatchStatsPage } from './pages/MatchStatsPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000 },
  },
})

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <div className="min-h-screen bg-slate-900 text-white">
            <Header />
            <main>
              <Routes>
                <Route path="/" element={<MatchListPage />} />
                <Route path="/matches/:id/live" element={<MatchLivePage />} />
                <Route path="/matches/:id/stats" element={<MatchStatsPage />} />
              </Routes>
            </main>
          </div>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  )
}

export default App
