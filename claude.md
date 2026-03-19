# Basket Stats - AI Guide

## Project Overview

**Basket Stats** is a project for analyzing and visualizing basketball statistics.

### Objectives
- Collect player and team data
- Calculate and store statistics
- Provide visualizations and reports

---

## AI Assistant Guidelines

### When Clarification Is Needed
- Feature scope: which functionalities are included?
- Design decisions: which approach is preferable?
- Data structure: how should data be organized?
- External dependencies: which libraries/APIs to use?

### Code Patterns
- Keep code simple and readable
- Add comments only when necessary
- Use descriptive naming
- Follow project conventions

### Workflow
1. **Explore**: Understand structure and context
2. **Plan**: Create a structured plan (if needed)
3. **Implement**: Make minimal and precise changes
4. **Validate**: Test changes to ensure they work

### Change Checklist
- [ ] Are changes minimal and focused?
- [ ] Does code follow project standards?
- [ ] Do tests pass (if any)?
- [ ] Has documentation been updated?

---

## Project Structure

### Directory Layout
```
basket-stats/
├── src/
│   ├── BasketStats.API/              # Presentation Layer
│   ├── BasketStats.Application/      # Application Layer (CQS)
│   ├── BasketStats.Domain/           # Domain Layer
│   └── BasketStats.Infrastructure/   # Infrastructure Layer
├── tests/
│   └── Unit/
│       ├── BasketStats.API.Tests/
│       ├── BasketStats.Application.Tests/
│       ├── BasketStats.Domain.Tests/
│       └── BasketStats.Infrastructure.Tests/
├── claude.md
├── BasketStats.sln
└── docker-compose.yml
```

### Architecture Layers

#### Domain Layer (`BasketStats.Domain`)
- **Entities**: Match, Event, User, Team
- **Value Objects**: MatchId, EventId, Coordinates, Period, Score
- **Repository Interfaces**: IMatchRepository, IEventRepository, IUserRepository
- **Business Rules**: Event validation, free throw calculation, period logic
- **No external dependencies** (no frameworks, no NuGet packages)

#### Application Layer (`BasketStats.Application`)
- **Commands**: CreateMatchCommand, AddEventCommand, UpdateMatchStatus
- **Queries**: GetMatchQuery, ListMatchesQuery, GetPlayerStatsQuery
- **Command/Query Handlers**: Implement use cases using MediatR
- **DTOs**: Application-level data transfer objects
- **Services**: Business logic orchestration

#### Infrastructure Layer (`BasketStats.Infrastructure`)
- **Repository Implementations**: Firestore-based repositories
- **Data Mappers**: Domain entity ↔ Firestore document mapping
- **External Services**: Keycloak authentication adapter, GCS integration
- **Firebase/Firestore Configuration**: Collection definitions, indexes

#### Presentation Layer (`BasketStats.API`)
- **Controllers**: Thin controllers delegating to MediatR
- **API DTOs**: Request/Response contracts
- **Middleware**: Authentication, CORS, error handling
- **Program.cs**: Dependency injection and service configuration
- **Swagger/OpenAPI**: API documentation

---

## Technology Stack

### Backend Architecture
- **Pattern**: Clean Architecture with CQS (Command Query Separation)
- **Framework**: ASP.NET Core 10
- **Language**: C# 13
- **Database**: Google Cloud Firestore (NoSQL)
- **Authentication**: Keycloak (OpenID Connect + JWT)
- **CQS Library**: MediatR (for command/query pattern)

### Project Structure
- **BasketStats.Domain**: Core business logic (DDD - Domain-Driven Design)
- **BasketStats.Application**: Use cases and CQS handlers (orchestration)
- **BasketStats.Infrastructure**: Data access and external integrations
- **BasketStats.API**: HTTP presentation layer and REST endpoints

### Key Dependencies
- **MediatR** (v13.0.0): CQS pattern implementation
- **Microsoft.AspNetCore.Authentication.OpenIdConnect**: Keycloak integration
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT token validation
- **Google.Cloud.Firestore**: Firestore database client
- **Google.Cloud.Storage.V1**: Cloud Storage integration
- **Serilog**: Structured logging
- **FluentValidation**: Input validation
- **xUnit + Moq**: Unit testing
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation

### Infrastructure
- **Keycloak**: Authentication and authorization (OAuth2/OpenID Connect)
- **Google Cloud Platform**:
  - Cloud Firestore: Document-oriented database
  - Cloud Run: Serverless API hosting
  - Cloud Storage: File storage
- **Docker**: Local development environment (Keycloak, Firestore Emulator, GCS Emulator)

---

## MVP - Requirements

### Main Features
1. **Match Management**
   - Receive new match data
   - Update match events in real-time (scores, fouls, substitutions)
   - Persist match history

2. **Authentication and Authorization**
   - Keycloak integration
   - Login via OpenID Connect
   - Role-based authorization (admin, match creator, viewer)

3. **REST API**
   - POST `/matches` - create match
   - PUT `/matches/{id}/events` - add events
   - GET `/matches/{id}` - get details
   - GET `/matches` - list matches

### Data Structure
- **Match**: id, teams, players, startTime, status, events[], periods[] (per-period data), teamFouls[] (per period)
- **Period**: number (1-4), quarterStartTime, quarterEndTime, duration (in seconds)
- **Event**: timestamp (absolute), periodNumber, periodTimestamp (time within period in seconds), type (score, missed_shot, free_throw, foul, substitution), details, teamId, playerId
- **ScoreEvent**: points (2 or 3), coordinates {x, y}, playerName
- **MissedShotEvent**: coordinates {x, y}, playerName
- **FreeThrowEvent**: made (boolean), foulType (personal, technical, flagrant), playerName
- **FoulEvent**: foulType, playerFouled, flagrant (boolean)
- **User**: id, email, keycloakId, roles[]

---

## Business Rules

### Authentication & Authorization
1. **Keycloak OpenID Connect**
   - Primary authentication provider
   - User credentials stored in Keycloak
   - JWT tokens issued for API requests

2. **Google OAuth Integration (Optional)**
   - Users can authenticate via Google account
   - Google email linked to Keycloak user
   - Requires Google OAuth credentials
   - Setup: See `.docker/README.md` for configuration steps
1. **Team Ownership & Event Types**
   - Team owner can register: Score, Missed Shot, Free Throw, Foul, Substitution for their team
   - Opponent can register: Score, Missed Shot, and Free Throw for opponent team
   - Non-participants cannot register any events

2. **Shot Coordinates Requirement**
   - Score events (made shots) MUST include coordinates (x, y)
   - Missed shot events MUST include coordinates (x, y)
   - Free throws do NOT require coordinates (arremessos da linha de lance)
   - Coordinates represent position on court where shot was attempted
   - User selects position on visual court image (UI component)
   - Only coordinates {x, y} stored in database (NOT the image)
   - Coordinates used to generate player heat maps showing:
     - Made shots (successful attempts)
     - Missed shots (failed attempts)
     - Overall shooting patterns

3. **Free Throw Rules (FIBA)**
   - Award 1, 2, or 3 free throws based on foul type:
     - Personal foul (no bonus): 1 free throw
     - Personal foul (team bonus - 4+ fouls): 2 free throws
     - Technical foul: 1 free throw
     - Flagrant foul: 2 free throws + ball possession
   - Each free throw: 1 point (made) or 0 points (missed)
   - Tracked separately: made free throws vs missed free throws
   - NO coordinates (linha de lance is fixed position)

4. **Period & Time Tracking**
   - Match consists of 4 periods (10 minutes each in FIBA)
   - Each period has: number, startTime, endTime
   - Each event includes:
     - Absolute timestamp (when event occurred)
     - Period number (1-4)
     - Period timestamp (time within period in seconds)
   - Period timestamps used to:
     - Generate score evolution graphs (points over time)
     - Analyze performance by period
     - Create timeline visualizations
   - Validate periodTimestamp is within period duration

5. **Match Lifecycle**
   - Scheduled → Active → Finished (valid state transitions)
   - Events can only be added to Active matches
   - Cannot add events to Finished matches

---

## Next Steps

### Immediate (Phase 1-2)
1. Create Domain, Application, Infrastructure class libraries
2. Add MediatR NuGet packages
3. Create domain entities and value objects
4. Define repository interfaces

### Short Term (Phase 3-4)
5. Implement command and query handlers
6. Create Firestore repository implementations
7. Configure dependency injection

### Medium Term (Phase 5-6)
8. Refactor controllers to use CQS
9. Add comprehensive tests
10. Update API documentation

### Long Term
11. Add integration tests with Firestore emulator
12. Implement caching strategy
13. Add event sourcing (future enhancement)
14. Implement saga pattern for complex workflows (if needed)

---

## Contact & Notes

- Communication Language: English
- Focus: Code quality and clarity
