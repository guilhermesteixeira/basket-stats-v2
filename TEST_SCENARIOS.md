# Basket Stats MVP - Test Scenarios Plan

## Overview
Define comprehensive test scenarios for MVP implementation covering:
1. Authentication (Keycloak integration)
2. Match Management (CRUD operations)
3. Event Management (real-time updates)
4. Authorization (role-based access)
5. Data Validation

---

## Test Categories

### 1. Authentication Tests (Keycloak)
#### Login Scenarios
- TC-AUTH-001: Successful login with valid credentials
- TC-AUTH-002: Failed login with invalid email
- TC-AUTH-003: Failed login with invalid password
- TC-AUTH-004: Login with non-existent user
- TC-AUTH-005: Token validation and expiration

#### Authorization Scenarios
- TC-AUTH-006: User with admin role can access admin endpoints
- TC-AUTH-007: User without admin role cannot access admin endpoints
- TC-AUTH-008: Team owner can add all event types to own team
- TC-AUTH-009: Opponent can only add score events to other team (not fouls/substitutions)
- TC-AUTH-010: Non-involved user cannot add any events to match

---

### 2. Match Management Tests (CRUD)
#### Create Match
- TC-MATCH-001: Successfully create match with valid data
- TC-MATCH-002: Fail to create with missing required fields (teams, players)
- TC-MATCH-003: Fail to create with invalid team data
- TC-MATCH-004: Fail to create with duplicate teams
- TC-MATCH-005: Create match with minimum required fields

#### Read Match
- TC-MATCH-006: Retrieve existing match by ID
- TC-MATCH-007: Fail to retrieve non-existent match (404)
- TC-MATCH-008: List all matches with pagination
- TC-MATCH-009: Filter matches by team
- TC-MATCH-010: Filter matches by status (active, finished, scheduled)

#### Update Match
- TC-MATCH-011: Update match status (scheduled → active → finished)
- TC-MATCH-012: Fail to update with invalid status transition
- TC-MATCH-013: Fail to update non-existent match
- TC-MATCH-014: Only creator/admin can update match

#### Delete Match
- TC-MATCH-015: Successfully delete match (admin only)
- TC-MATCH-016: Fail to delete with insufficient permissions
- TC-MATCH-017: Cascade delete associated events

---

### 3. Event Management Tests

#### Event Ownership & Authorization Rules
**KEY REQUIREMENT**: 
- Team owner can register: Score, Missed Shot, Free Throw, Foul, Substitution for their team
- Opponent can register: Score, Missed Shot, and Free Throw for opponent team
- Non-participants cannot register any events

#### Free Throw Requirements (FIBA)
**KEY REQUIREMENT**:
- Free throw events based on foul type:
  - Personal foul (no bonus): 1 free throw
  - Personal foul (team bonus - 4+ fouls): 2 free throws
  - Technical foul: 1 free throw
  - Flagrant foul: 2 free throws + ball possession
- Each free throw registered separately (made or missed)
- 1 point per made free throw
- NO coordinates (linha de lance is fixed position)
- Tracked as separate metric: made vs missed free throws

#### Shot Coordinates Requirement
**KEY REQUIREMENT**: 
- Score events (2 or 3 points) must include shot coordinates (x, y)
- Missed shot events must include shot coordinates (x, y)
- Coordinates represent position on court where shot was attempted
- User clicks on visual court image to select position
- Only coordinates stored in database (NOT the image itself)
- Coordinates used to generate player heat maps showing:
  - Made shots (successful attempts)
  - Missed shots (failed attempts)
  - Overall shooting patterns
- Reject score/missed shot events without coordinates

#### Add Event
- TC-EVENT-001: Add score event to own team (authorized)
- TC-EVENT-002: Add foul event to own team (authorized)
- TC-EVENT-003: Add substitution event to own team (authorized)
- TC-EVENT-004: Add missed shot event to own team (authorized)
- TC-EVENT-005: Add free throw event to own team (authorized)
- TC-EVENT-007: Add score event to opponent team (authorized)
- TC-EVENT-008: Add missed shot to opponent team (authorized)
- TC-EVENT-009: Add free throw to opponent team (authorized)
- TC-EVENT-010: Deny foul event to opponent team (unauthorized)
- TC-EVENT-011: Deny substitution to opponent team (unauthorized)
- TC-EVENT-012: Verify event ownership restrictions enforced
- TC-EVENT-013: Score event requires coordinates (2-pointer)
- TC-EVENT-014: Score event requires coordinates (3-pointer)
- TC-EVENT-015: Missed shot event requires coordinates
- TC-EVENT-016: Reject score without coordinates
- TC-EVENT-017: Reject missed shot without coordinates
- TC-EVENT-018: Free throw does NOT require coordinates
- TC-EVENT-019: Foul event does NOT require coordinates
- TC-EVENT-020: Substitution does NOT require coordinates
- TC-EVENT-021: Fail to add event to finished match
- TC-EVENT-022: Fail to add event to non-existent match
- TC-EVENT-023: Events stored in chronological order

#### Free Throw Tests
- TC-EVENT-024: Add 1 free throw (personal foul, no bonus)
- TC-EVENT-025: Add 2 free throws (personal foul, team bonus)
- TC-EVENT-026: Add 1 free throw (technical foul)
- TC-EVENT-027: Add 2 free throws (flagrant foul)
- TC-EVENT-028: Made free throw = 1 point
- TC-EVENT-029: Missed free throw = 0 points
- TC-EVENT-030: Track made vs missed free throws separately
- TC-EVENT-031: Free throw does NOT require coordinates
- TC-EVENT-032: Count team fouls per period for bonus calculation
- TC-EVENT-033: Trigger 2 free throws after 4 team fouls (bonus)

#### Event Retrieval
- TC-EVENT-034: Get all events for a match
- TC-EVENT-035: Get events filtered by type (score, missed, free_throw, foul, substitution)
- TC-EVENT-036: Events returned in chronological order
- TC-EVENT-037: Pagination support for large event lists

#### Coordinate Storage
- TC-EVENT-038: Store shot coordinates (x, y) for score event
- TC-EVENT-039: Store shot coordinates (x, y) for missed shot
- TC-EVENT-040: Coordinates in valid court range (0-100)
- TC-EVENT-041: Generate heat map from made shots
- TC-EVENT-042: Generate heat map from missed shots
- TC-EVENT-043: Generate combined heat map (made + missed)
- TC-EVENT-044: Filter heat map by player
- TC-EVENT-045: Filter heat map by team

---

### 4. Data Validation Tests
#### Match Validation
- TC-DATA-001: Team names cannot be empty
- TC-DATA-002: Player count minimum met (team must have players)
- TC-DATA-003: No duplicate players in team
- TC-DATA-004: Valid status values (scheduled, active, finished)
- TC-DATA-005: Start time cannot be in past

#### User Validation
- TC-DATA-006: Email format validation
- TC-DATA-007: Role must be from predefined list (admin, creator, viewer)
- TC-DATA-008: User ID mapped correctly from Keycloak

---

### 5. Integration Tests
#### API Contract
- TC-INT-001: All endpoints return correct HTTP status codes
- TC-INT-002: Error responses have consistent format
- TC-INT-003: Success responses match defined schema
- TC-INT-004: Request validation errors return 400
- TC-INT-005: Authorization errors return 401/403

#### Data Persistence
- TC-INT-006: Match persisted in Firestore
- TC-INT-007: Events persisted with correct relationships
- TC-INT-008: Concurrent event additions handled correctly
- TC-INT-009: Transactions ensure data consistency

---

### 6. Performance Tests (Future)
- TC-PERF-001: List 1000 matches with pagination
- TC-PERF-002: Add 100 events to single match
- TC-PERF-003: Query matches by team with large dataset
- TC-PERF-004: Cloud Run startup time < 5s

---

## Test Implementation Strategy

### Unit Tests
- Domain models validation
- Service business logic
- Event processing

### Integration Tests
- API endpoints
- Database operations
- Keycloak authentication flow

### Test Framework
- xUnit or NUnit for .NET Core
- Moq for mocking
- TestContainers for database testing (Firestore emulator)

---

## Testing Priorities (MVP)
1. **High Priority**: Authentication, Match CRUD, Event operations
2. **Medium Priority**: Authorization, Data validation, API contracts
3. **Low Priority**: Performance, edge cases

---

## Notes
- Use Firestore emulator locally for testing
- Mock Keycloak responses for unit tests
- Integration tests use real Keycloak test instance
- Aim for >80% code coverage for critical paths
