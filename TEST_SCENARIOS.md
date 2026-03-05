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
- Team owner can register: Score, Foul, Substitution for their team
- Opponent can register: Only Score for opponent team
- Non-participants cannot register any events

#### Shot Image Requirement
**KEY REQUIREMENT**: 
- Score events (2 or 3 points) must include image showing shot location
- Image used to generate heat map of court
- Image stored in Google Cloud Storage

#### Add Event
- TC-EVENT-001: Add score event to own team (authorized)
- TC-EVENT-002: Add foul event to own team (authorized)
- TC-EVENT-003: Add substitution event to own team (authorized)
- TC-EVENT-007: Add score event to opponent team (authorized)
- TC-EVENT-008: Deny foul event to opponent team (unauthorized)
- TC-EVENT-009: Deny substitution to opponent team (unauthorized)
- TC-EVENT-010: Verify event ownership restrictions enforced
- TC-EVENT-011: Score event requires image (2-pointer)
- TC-EVENT-012: Score event requires image (3-pointer)
- TC-EVENT-013: Reject score without image
- TC-EVENT-014: Foul event does NOT require image
- TC-EVENT-015: Substitution does NOT require image
- TC-EVENT-004: Fail to add event to finished match
- TC-EVENT-005: Fail to add event to non-existent match
- TC-EVENT-006: Events stored in correct chronological order

#### Event Retrieval
- TC-EVENT-016: Get all events for a match
- TC-EVENT-017: Get events filtered by type
- TC-EVENT-018: Events returned in chronological order
- TC-EVENT-019: Pagination support for large event lists

#### Image Storage
- TC-EVENT-020: Upload image to Cloud Storage successfully
- TC-EVENT-021: Generate image URL for shot location
- TC-EVENT-022: Image validation (format, size)

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
