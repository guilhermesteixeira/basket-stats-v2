# Basket Stats — Frontend Architecture

## Stack

| Concern | Technology |
|---|---|
| Framework | React 19 + TypeScript |
| Build tool | Vite |
| Styling | Tailwind CSS |
| Routing | React Router v7 |
| Server state | TanStack Query (React Query v5) |
| Client state | Zustand |
| Authentication | `@keycloak/keycloak-js` |
| Testing | Vitest + React Testing Library |
| HTTP client | Axios (with interceptor for JWT injection) |

---

## Project Structure

```
frontend/
├── public/
├── src/
│   ├── api/                    # Axios instance + per-resource API functions
│   │   ├── client.ts           # Axios instance + auth interceptor
│   │   ├── matches.ts          # match API calls
│   │   ├── teams.ts            # team API calls
│   │   ├── users.ts            # user API calls
│   │   └── events.ts           # event API calls
│   ├── auth/
│   │   ├── keycloak.ts         # Keycloak instance (singleton)
│   │   └── AuthProvider.tsx    # Context: token, user info, logout
│   ├── components/             # Reusable UI components
│   │   ├── ui/                 # Generic: Button, Badge, Card, Spinner
│   │   ├── layout/             # Header, Sidebar, PageWrapper
│   │   └── domain/             # Domain-specific: MatchCard, EventForm, CourtMap
│   ├── pages/                  # One file per route
│   │   ├── LoginPage.tsx
│   │   ├── TeamsNewPage.tsx
│   │   ├── MatchListPage.tsx
│   │   ├── MatchLivePage.tsx   # Live event recording
│   │   └── MatchStatsPage.tsx  # Statistics and charts
│   ├── hooks/                  # Custom hooks (TanStack Query wrappers)
│   │   ├── useMatches.ts
│   │   ├── useMatch.ts
│   │   ├── useAddEvent.ts
│   │   └── useTeams.ts
│   ├── store/                  # Zustand stores
│   │   └── matchStore.ts       # Active match state (live page)
│   ├── types/                  # TypeScript types mirroring API contracts
│   │   ├── match.ts
│   │   ├── event.ts
│   │   ├── team.ts
│   │   └── user.ts
│   ├── utils/
│   │   └── eventHelpers.ts     # Free throw count, foul bonus logic
│   ├── App.tsx                 # Router setup
│   └── main.tsx                # Keycloak init → render App
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

---

## Screens

### 1. Login (`/login`)

- Keycloak JS adapter in `implicit` or `standard` flow
- On app load: if not authenticated → `keycloak.login()` redirect
- After login: call `POST /api/users/me` to auto-register in Firestore
- Show loading spinner while token is being initialised

### 2. Team Registration (`/teams/new`)

- Simple form: team name input
- Calls `POST /api/teams`
- On success: redirect to match list
- Requires authentication

### 3. Match List (`/`)

- Calls `GET /api/matches` (public endpoint)
- Filter buttons: All / Scheduled / Active / Finished
- Each match card shows: home team vs away team, status badge, start button (if owner)
- Create match button (if authenticated + match-creator role)

### 4. Live Event Recording (`/matches/:id/live`)

- Calls `GET /api/matches/:id` to load current state
- Polls every 10s (or manual refresh) — future: WebSocket/SSE
- Two panels: Home team | Away team
- Event type buttons: Score, Missed Shot, Free Throw, Foul, Substitution
- Court map component for selecting coordinates (Score, Missed Shot)
- Period selector + period timestamp input
- Authorization-aware: only shows allowed actions based on ownership
- Calls `POST /api/matches/:id/events`
- Start / Finish match buttons (owner only)

### 5. Match Statistics (`/matches/:id/stats`)

- Score per period table
- Team foul counts per period
- Point timeline chart (area chart per team over period timestamps)
- Player heat map — made shots (green) + missed shots (red) on court image
- Event log (all events in chronological order)

---

## Authentication Flow

```
main.tsx
  └── keycloak.init({ onLoad: 'login-required' })
        ├── success: set token in Zustand / Axios interceptor
        │           call POST /api/users/me
        │           render <App />
        └── error:   show error page

Axios interceptor (client.ts)
  └── request: inject Authorization: Bearer <keycloak.token>
  └── response 401: keycloak.updateToken(30) → retry once → keycloak.login()
```

---

## State Management

### TanStack Query (server state)
- All API data: matches, match detail, team list
- Automatic cache invalidation after mutations (addEvent → refetch match)
- Query keys: `['matches']`, `['match', id]`, `['teams']`

### Zustand (client/UI state)
- `matchStore`: current period selection, selected event type, live poll interval
- No duplication with server state — only transient UI state

---

## API Integration

### Types (mirror backend contracts)

```typescript
// types/match.ts
export type MatchStatus = 'Scheduled' | 'Active' | 'Finished';

export interface Match {
  id: string;
  homeTeamId: string;
  awayTeamId: string;
  status: MatchStatus;
  startedAt?: string;
  finishedAt?: string;
  events: Event[];
  periods: Period[];
}

// types/event.ts
export type EventType = 'Score' | 'MissedShot' | 'FreeThrow' | 'Foul' | 'Substitution';
export type FoulType = 'Personal' | 'Technical' | 'Flagrant';
export type PeriodNumber = 'One' | 'Two' | 'Three' | 'Four';

export interface AddEventPayload {
  teamId: string;
  playerId: string;
  type: EventType;
  periodNumber: PeriodNumber;
  periodTimestamp: number;
  coordinatesX?: number;
  coordinatesY?: number;
  points?: number;
  made?: boolean;
  foulType?: FoulType;
  playerFouledId?: string;
  flagrant?: boolean;
  playerOutId?: string;
}
```

---

## Environment Configuration

```env
# frontend/.env.local
VITE_API_BASE_URL=http://localhost:5273
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=basket-stats
VITE_KEYCLOAK_CLIENT_ID=basket-stats-api
```

---

## Local Development

```bash
cd frontend
npm install
npm run dev       # http://localhost:5174
npm run test      # Vitest unit/component tests
npm run build     # Production build
```

---

## Testing Strategy

See `TEST_SCENARIOS.md` — Frontend section for full scenario list.

### Unit / Component tests (Vitest + RTL)
- Hook tests: `useAddEvent`, `useMatches`
- Component tests: `EventForm`, `CourtMap`, `MatchCard`
- Auth guard: redirect when unauthenticated

### E2E (Playwright — future)
- Login → register user → create team → create match → add event

---

## CORS Configuration

The API already permits `http://localhost:5174` (Vite default port). For production,
update `Program.cs` CORS policy to include the deployed frontend URL.
