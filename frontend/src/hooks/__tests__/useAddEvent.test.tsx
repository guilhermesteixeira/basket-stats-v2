import { renderHook } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAddEvent } from '../useAddEvent'
import type { ReactNode } from 'react'

vi.mock('../useNetworkStatus')
vi.mock('../../api/matches', () => ({ addEvent: vi.fn() }))
vi.mock('../../offline/eventQueue', () => ({ enqueueEvent: vi.fn() }))

import { useNetworkStatus } from '../useNetworkStatus'
import { addEvent } from '../../api/matches'
import { enqueueEvent } from '../../offline/eventQueue'
import type { AddEventRequest } from '../../types'

const mockUseNetworkStatus = vi.mocked(useNetworkStatus)
const mockAddEvent = vi.mocked(addEvent)
const mockEnqueueEvent = vi.mocked(enqueueEvent)

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

const sampleRequest: AddEventRequest = {
  type: 'Score',
  teamId: 'team-1',
  periodNumber: 1,
  periodTimestamp: 30,
  details: { points: 2 },
}

describe('useAddEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('calls addEvent when online', async () => {
    mockUseNetworkStatus.mockReturnValue({ online: true })
    mockAddEvent.mockResolvedValue({
      id: 'evt-1',
      type: 'Score',
      teamId: 'team-1',
      periodNumber: 1,
      periodTimestamp: 30,
      timestamp: '',
      details: { points: 2 },
    })

    const { result } = renderHook(() => useAddEvent('match-1'), { wrapper })

    await new Promise<void>((resolve) => {
      result.current.mutate(sampleRequest, { onSuccess: () => resolve() })
    })

    expect(mockAddEvent).toHaveBeenCalledWith('match-1', sampleRequest)
    expect(mockEnqueueEvent).not.toHaveBeenCalled()
  })

  it('calls enqueueEvent when offline', async () => {
    mockUseNetworkStatus.mockReturnValue({ online: false })
    mockEnqueueEvent.mockResolvedValue(undefined)

    const { result } = renderHook(() => useAddEvent('match-1'), { wrapper })

    await new Promise<void>((resolve) => {
      result.current.mutate(sampleRequest, { onSuccess: () => resolve() })
    })

    expect(mockEnqueueEvent).toHaveBeenCalledWith('match-1', sampleRequest)
    expect(mockAddEvent).not.toHaveBeenCalled()
  })
})
