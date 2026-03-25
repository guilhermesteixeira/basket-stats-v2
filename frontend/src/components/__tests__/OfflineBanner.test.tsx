import { render, screen } from '@testing-library/react'
import { OfflineBanner } from '../../components/offline/OfflineBanner'

describe('OfflineBanner', () => {
  it('renders the offline message', () => {
    render(<OfflineBanner />)
    expect(
      screen.getByText(/You are offline/i),
    ).toBeInTheDocument()
  })

  it('shows pending count badge when pendingCount > 0', () => {
    render(<OfflineBanner pendingCount={3} />)
    expect(screen.getByText(/3 pending/i)).toBeInTheDocument()
  })

  it('does not show pending count badge when pendingCount is 0', () => {
    render(<OfflineBanner pendingCount={0} />)
    expect(screen.queryByText(/pending/i)).not.toBeInTheDocument()
  })

  it('does not show pending count badge by default', () => {
    render(<OfflineBanner />)
    expect(screen.queryByText(/pending/i)).not.toBeInTheDocument()
  })
})
