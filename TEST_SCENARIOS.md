# Basket Stats MVP - Test Scenarios Plan

## Overview

Define comprehensive test scenarios for MVP implementation covering:
1. User & Team Registration
2. Authentication (Keycloak integration)
3. Match Management
4. Event Management
5. Data Validation

**Legend**: ✅ Implemented · 🔲 Planned

---

## Test Summary

| Project | Tests | Status |
|---|---|---|
| `BasketStats.Domain.Tests` | 88 | ✅ All passing |
| `BasketStats.Application.Tests` | 51 | ✅ All passing |
| `BasketStats.API.Tests` | 14 | ✅ All passing |
| `BasketStats.Infrastructure.Tests` | 19 | ✅ All passing |
| `BasketStats.Integration.Tests` | — | Requires Firestore emulator |

---

## Test Categories

### 1. User & Team Registration

> **Note on identity**: `RequestedByUserId` in all commands carries the Keycloak JWT `sub` claim.
> Handlers resolve the internal Firestore user ID via `GetByKeycloakIdAsync`. Team `OwnerId`
> is stored as the Firestore user ID (not the Keycloak sub).

#### Create User (`POST /api/users/me`)
- ✅ TC-USER-001: New user is created and returns internal Firestore ID
- ✅ TC-USER-002: Existing user (same Keycloak sub) returns existing ID without duplicate save
- ✅ TC-USER-003: Internal Firestore ID is a new GUID, distinct from the Keycloak sub

#### Create Team (`POST /api/teams`)
- ✅ TC-TEAM-001: Valid request creates team; OwnerId = requester's Firestore user ID
- ✅ TC-TEAM-002: Unknown Keycloak sub throws NotFoundException
- ✅ TC-TEAM-003: OwnerId is Firestore user ID (not Keycloak sub)
- ✅ TC-TEAM-004: Same owner can create multiple teams with distinct IDs

---

### 2. Authentication Tests (Keycloak)

#### Login Scenarios
- 🔲 TC-AUTH-001: Successful login with valid credentials
- 🔲 TC-AUTH-002: Failed login with invalid email
- 🔲 TC-AUTH-003: Failed login with invalid password
- 🔲 TC-AUTH-004: Login with non-existent user
- 🔲 TC-AUTH-005: Token validation and expiration

#### Authorization Scenarios
- 🔲 TC-AUTH-006: User with admin role can access admin endpoints
- 🔲 TC-AUTH-007: User without admin role cannot access admin endpoints
- ✅ TC-AUTH-008: Team owner can add all event types to own team
- ✅ TC-AUTH-009: Opponent can add Score, MissedShot, FreeThrow to other team — but not Foul or Substitution
- ✅ TC-AUTH-010: Non-involved user cannot add any events to match (ForbiddenException)
- ✅ TC-AUTH-011: Admin user bypasses authorization and can add any event type

---

### 3. Match Management Tests

#### Create Match (`POST /api/matches`)
- ✅ TC-MATCH-001: Successfully create match with valid team IDs → returns match ID
- ✅ TC-MATCH-002: Home team not found → NotFoundException
- ✅ TC-MATCH-003: Away team not found → NotFoundException
- ✅ TC-MATCH-004: Home and away team are the same → InvalidOperationException
- 🔲 TC-MATCH-005: Requesting user not found → NotFoundException

#### Read Match (`GET /api/matches/{id}`, `GET /api/matches`)
- ✅ TC-MATCH-006: Retrieve existing match by ID
- ✅ TC-MATCH-007: Retrieve non-existent match → NotFoundException (404)
- ✅ TC-MATCH-008: List all matches returns collection

#### Match Lifecycle (`PUT /api/matches/{id}/start`, `PUT /api/matches/{id}/finish`)
- ✅ TC-MATCH-011: Scheduled → Active (start succeeds)
- ✅ TC-MATCH-012: Active → Active (start again) → InvalidOperationException
- ✅ TC-MATCH-013: Active → Finished (finish succeeds)
- ✅ TC-MATCH-014: Scheduled → Finished (skip start) → InvalidOperationException
- ✅ TC-MATCH-015: Match not found → NotFoundException

---

### 4. Event Management Tests

#### Event Ownership & Authorization Rules

**Business Rules:**
- Team owner can register: Score, MissedShot, FreeThrow, Foul, Substitution for **their** team
- Opponent can register: Score, MissedShot, FreeThrow for the **other** team
- Non-participants cannot register any events

#### Add Event to Own Team (authorized)
- ✅ TC-EVENT-001: Score event to own team
- ✅ TC-EVENT-002: Foul event to own team
- ✅ TC-EVENT-003: Substitution event to own team
- ✅ TC-EVENT-004: MissedShot event to own team (with coordinates)
- ✅ TC-EVENT-005: FreeThrow (made) to own team

#### Add Event to Opponent Team
- ✅ TC-EVENT-007: Score event to opponent team (authorized)
- ✅ TC-EVENT-008: MissedShot to opponent team (authorized)
- ✅ TC-EVENT-009: FreeThrow to opponent team (authorized)
- ✅ TC-EVENT-010: Foul to opponent team → ForbiddenException
- ✅ TC-EVENT-011: Substitution to opponent team → ForbiddenException

#### Shot Coordinates

**Business Rules:**
- Score (2pt or 3pt) events **must** include coordinates `{x, y}` in range 0–100
- MissedShot events **must** include coordinates
- FreeThrow, Foul, Substitution events do **not** require coordinates

- ✅ TC-EVENT-013: 2-point score requires coordinates
- ✅ TC-EVENT-014: 3-point score requires coordinates (domain: `ScoreEvent` validates points = 2 or 3)
- ✅ TC-EVENT-016: Score without coordinates → InvalidOperationException
- ✅ TC-EVENT-017: MissedShot without coordinates → InvalidOperationException
- ✅ TC-EVENT-018: FreeThrow without coordinates → succeeds
- ✅ TC-EVENT-019: Foul without coordinates → succeeds
- ✅ TC-EVENT-020: Substitution without coordinates → succeeds
- ✅ TC-EVENT-038: Coordinates `{x, y}` stored on ScoreEvent
- ✅ TC-EVENT-039: Coordinates `{x, y}` stored on MissedShotEvent
- ✅ TC-EVENT-040: Coordinates out of 0–100 range → ArgumentException (domain)

#### Match State Guards
- ✅ TC-EVENT-021: Add event to finished match → InvalidOperationException
- ✅ TC-EVENT-022: Add event to non-existent match → NotFoundException

#### Period & Timestamp

**Business Rules:**
- Each event carries `PeriodNumber` (1–4) and `PeriodTimestamp` (0–600 seconds within period)

- ✅ TC-EVENT-047: Event records the correct period number (1–4)
- ✅ TC-EVENT-049: PeriodTimestamp 0–600 → valid
- ✅ TC-EVENT-050: PeriodTimestamp > 600 → ArgumentException

#### Free Throw

**Business Rules (FIBA):**
- Personal foul (no bonus): 1 free throw
- Personal foul (team bonus — 4+ fouls in period): 2 free throws
- Technical foul: 1 free throw
- Flagrant foul: 2 free throws + ball possession

- ✅ TC-EVENT-024: FreeThrow with `Made = true` is recorded
- ✅ TC-EVENT-029: FreeThrow with `Made = false` is recorded
- ✅ TC-EVENT-032: `Match.GetTeamFoulCount` counts fouls per team per period
- ✅ TC-EVENT-033: Fouls in different periods are counted independently

#### Event Domain — Value Objects
- ✅ TC-SCORE-001: ScoreEvent with 2 points succeeds
- ✅ TC-SCORE-002: ScoreEvent with 3 points succeeds
- ✅ TC-SCORE-003: ScoreEvent with 1, 4, or 5 points → ArgumentException
- ✅ TC-SCORE-004: ScoreEvent without coordinates → ArgumentNullException
- ✅ TC-MISSED-001: MissedShotEvent with coordinates succeeds
- ✅ TC-MISSED-002: MissedShotEvent without coordinates → ArgumentNullException
- ✅ TC-FREE-001: FreeThrowEvent (made) succeeds
- ✅ TC-FREE-002: FreeThrowEvent (missed) succeeds
- ✅ TC-FREE-003: FreeThrow with Personal, Technical foul types succeeds
- ✅ TC-FOUL-001: FoulEvent (personal) succeeds
- ✅ TC-FOUL-002: FoulEvent (flagrant) sets `Flagrant = true`
- ✅ TC-FOUL-003: FoulEvent with empty fouled-player ID → ArgumentException
- ✅ TC-SUB-001: SubstitutionEvent succeeds
- ✅ TC-SUB-002: SubstitutionEvent with empty player-out ID → ArgumentException
- ✅ TC-EVT-BASE-001: Event with empty teamId → ArgumentException
- ✅ TC-EVT-BASE-002: Event with empty playerId → ArgumentException
- ✅ TC-EVT-BASE-003: PeriodTimestamp 0, 300, 600 → valid
- ✅ TC-EVT-BASE-004: PeriodTimestamp -1 or 601 → ArgumentException

---

### 5. Data Validation Tests — Domain

#### Match Entity
- ✅ TC-DATA-001: Match created with valid teams — status = Scheduled, 4 periods, no events
- ✅ TC-DATA-002: Empty homeTeamId → ArgumentException
- ✅ TC-DATA-003: Empty awayTeamId → ArgumentException
- ✅ TC-DATA-004: Same team IDs → InvalidOperationException
- ✅ TC-DATA-005: Start scheduled match → status = Active, StartedAt set
- ✅ TC-DATA-006: Start already-active match → InvalidOperationException
- ✅ TC-DATA-007: Finish active match → status = Finished, FinishedAt set
- ✅ TC-DATA-008: Finish scheduled match → InvalidOperationException

#### Team Entity
- ✅ TC-DATA-009: Valid team creation sets Id, Name, OwnerId
- ✅ TC-DATA-010: Empty Id, Name, or OwnerId → ArgumentException
- ✅ TC-DATA-011: `IsOwnedBy` correct owner → true
- ✅ TC-DATA-012: `IsOwnedBy` wrong owner → false

#### User Entity
- ✅ TC-DATA-013: Valid user creation sets Id, Email, Name, KeycloakId, empty Roles
- ✅ TC-DATA-014: Empty Id, Email, Name, or KeycloakId → ArgumentException
- ✅ TC-DATA-015: AddRole adds role to list
- ✅ TC-DATA-016: AddRole duplicate → no duplicate stored
- ✅ TC-DATA-017: AddRole empty string → ArgumentException
- ✅ TC-DATA-018: HasRole with existing role → true
- ✅ TC-DATA-019: HasRole with non-existing role → false
- ✅ TC-DATA-020: `IsMatchCreator` / `IsAdmin` reflect roles correctly

#### Value Objects
- ✅ TC-VO-001: Coordinates 0–100 range → valid
- ✅ TC-VO-002: Coordinates outside 0–100 → ArgumentException
- ✅ TC-VO-003: Coordinates equality by value
- ✅ TC-VO-004: Period created with valid PeriodNumber → IsActive = true
- ✅ TC-VO-005: Period.End sets EndTime, IsActive = false
- ✅ TC-VO-006: Period.End before start → InvalidOperationException
- ✅ TC-VO-007: Period.ElapsedSeconds capped at 600s

---

### 6. Integration Tests (`BasketStats.Integration.Tests`)

> Require Firestore emulator running (`docker-compose up`).

#### Repository — Match
- 🔲 TC-INT-MATCH-001: Save and retrieve match by ID
- 🔲 TC-INT-MATCH-002: List all matches returns persisted matches
- 🔲 TC-INT-MATCH-003: Save match with events — events persisted

#### Repository — Team
- 🔲 TC-INT-TEAM-001: Save and retrieve team by ID
- 🔲 TC-INT-TEAM-002: Non-existent team ID returns null

#### Repository — User
- 🔲 TC-INT-USER-001: Save and retrieve user by Keycloak ID
- 🔲 TC-INT-USER-002: Non-existent Keycloak ID returns null

---

### 7. Frontend Tests — Component (Vitest + React Testing Library)

> Test file location: `frontend/src/**/__tests__/` or `*.test.tsx`

#### Authentication & Routing
- 🔲 TC-FE-AUTH-001: Unauthenticated user is redirected to Keycloak login
- 🔲 TC-FE-AUTH-002: After login, `POST /api/users/me` is called automatically
- 🔲 TC-FE-AUTH-003: JWT token is injected in all Axios requests (Authorization header)
- 🔲 TC-FE-AUTH-004: 401 response triggers token refresh; on failure redirects to login

#### Team Registration (`/teams/new`)
- 🔲 TC-FE-TEAM-001: Form renders with team name input and submit button
- 🔲 TC-FE-TEAM-002: Submit with empty name shows validation error
- 🔲 TC-FE-TEAM-003: Successful submission calls `POST /api/teams` and redirects
- 🔲 TC-FE-TEAM-004: API error shows user-friendly error message

#### Match List (`/`)
- 🔲 TC-FE-MATCH-001: Renders list of matches from `GET /api/matches`
- 🔲 TC-FE-MATCH-002: Loading state shown while fetching
- 🔲 TC-FE-MATCH-003: Empty state shown when no matches exist
- 🔲 TC-FE-MATCH-004: Status filter (All/Scheduled/Active/Finished) filters visible matches
- 🔲 TC-FE-MATCH-005: Match card shows home team, away team, and status badge
- 🔲 TC-FE-MATCH-006: "Create match" button only shown to authenticated match-creator

#### Live Event Recording (`/matches/:id/live`)
- 🔲 TC-FE-LIVE-001: Loads match data from `GET /api/matches/:id`
- 🔲 TC-FE-LIVE-002: Score and MissedShot buttons open court map for coordinate selection
- 🔲 TC-FE-LIVE-003: FreeThrow form shows Made/Missed toggle and foul type selector
- 🔲 TC-FE-LIVE-004: Foul form shows player fouled input and flagrant checkbox
- 🔲 TC-FE-LIVE-005: Substitution form shows player-in and player-out inputs
- 🔲 TC-FE-LIVE-006: Period selector allows choosing period 1–4
- 🔲 TC-FE-LIVE-007: Submitting event calls `POST /api/matches/:id/events` and refreshes match
- 🔲 TC-FE-LIVE-008: Owner sees all 5 event types for own team
- 🔲 TC-FE-LIVE-009: Opponent sees only Score, MissedShot, FreeThrow buttons for other team
- 🔲 TC-FE-LIVE-010: 403 response shows "not authorised" message
- 🔲 TC-FE-LIVE-011: Start Match button visible and functional for match owner (Scheduled match)
- 🔲 TC-FE-LIVE-012: Finish Match button visible for match owner (Active match)
- 🔲 TC-FE-LIVE-013: Event buttons disabled when match is Finished

#### Court Map Component
- 🔲 TC-FE-COURT-001: Clicking on court area sets coordinatesX and coordinatesY
- 🔲 TC-FE-COURT-002: Coordinates normalised to 0–100 range
- 🔲 TC-FE-COURT-003: Selected position shown as marker on court
- 🔲 TC-FE-COURT-004: Coordinate value changes on different click positions

#### Match Statistics (`/matches/:id/stats`)
- 🔲 TC-FE-STATS-001: Score per period table renders correctly
- 🔲 TC-FE-STATS-002: Team foul counts per period shown
- 🔲 TC-FE-STATS-003: Score timeline chart renders with events data
- 🔲 TC-FE-STATS-004: Player heat map renders made shots (green) and missed shots (red)
- 🔲 TC-FE-STATS-005: Event log shows all events in chronological order
- 🔲 TC-FE-STATS-006: Empty state shown when match has no events

---

### 8. Performance Tests (Future)
- 🔲 TC-PERF-001: List 1000 matches with pagination
- 🔲 TC-PERF-002: Add 100 events to single match
- 🔲 TC-PERF-003: Query matches by team with large dataset
- 🔲 TC-PERF-004: Cloud Run startup time < 5s

---

## Test Framework

### Backend
- **Unit tests**: xUnit + Moq
- **Integration tests**: xUnit + Firestore emulator (`docker-compose up`)
- **Test runner**: `~/.dotnet/dotnet test <project.csproj>`

### Frontend (planned)
- **Unit/Component tests**: Vitest + React Testing Library
- **Test runner**: `npm run test` (inside `frontend/`)

## Testing Priorities (MVP)

1. **High Priority** (implemented): User/Team registration, Match lifecycle, Event authorization, Coordinates, Period timestamps
2. **Medium Priority** (planned): Keycloak auth flows, Frontend component tests, API contract/data persistence
3. **Low Priority** (future): Performance, heat maps, pagination, E2E

## Notes

- Controllers pass the JWT `sub` claim as `RequestedByUserId` to all commands
- Handlers resolve the internal Firestore user via `GetByKeycloakIdAsync` — never `GetByIdAsync`
- Team `OwnerId` stores the Firestore user ID; `IsOwnedBy` must compare against `user.Id`, not the Keycloak sub
- Integration tests require Firestore emulator: `docker-compose up firestore`
