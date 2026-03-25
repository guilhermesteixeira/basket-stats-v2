# .NET 10 ASP.NET Core — Backend Setup

## Project Structure

```
basket-stats/
├── src/
│   ├── BasketStats.API/              # Presentation layer — controllers, requests, middleware
│   │   ├── Controllers/
│   │   │   ├── MatchesController.cs  # Match CRUD + lifecycle + events
│   │   │   ├── TeamsController.cs    # Team registration
│   │   │   └── UsersController.cs    # User self-registration
│   │   ├── Requests/                 # API request models (records)
│   │   ├── BasketStats.API.http      # Happy-path HTTP test file
│   │   ├── Program.cs
│   │   └── BasketStats.API.csproj
│   ├── BasketStats.Application/      # Use cases — commands, queries, handlers (MediatR/CQS)
│   │   ├── Commands/                 # CreateMatch, CreateTeam, CreateUser, StartMatch, FinishMatch, AddEvent
│   │   ├── Queries/                  # GetMatch, ListMatches, GetPlayerStats
│   │   ├── Handlers/                 # One handler per command/query
│   │   ├── DTOs/                     # Response data transfer objects
│   │   └── Exceptions/               # NotFoundException, ForbiddenException
│   ├── BasketStats.Domain/           # Core business rules — zero external dependencies
│   │   ├── Entities/                 # Match, Team, User, Event (+ subtypes)
│   │   ├── ValueObjects/             # Coordinates, Period, MatchId, EventId
│   │   ├── Enums/                    # MatchStatus, EventType, FoulType, PeriodNumber
│   │   └── Abstractions/             # IMatchRepository, ITeamRepository, IUserRepository
│   └── BasketStats.Infrastructure/   # Data access — Firestore repositories + mappers
│       ├── Repositories/
│       └── Mapping/
├── tests/
│   ├── Unit/
│   │   ├── BasketStats.Domain.Tests/         # 88 tests — entities + value objects
│   │   ├── BasketStats.Application.Tests/    # 51 tests — command/query handlers
│   │   ├── BasketStats.API.Tests/            # 14 tests — controller layer
│   │   └── BasketStats.Infrastructure.Tests/ # 19 tests — Firestore mappers
│   └── Integration/
│       └── BasketStats.Integration.Tests/    # Requires Firestore emulator
├── .docker/
│   └── keycloak/
│       └── realm-export.json         # Auto-imported on docker-compose up
├── docker-compose.yml
└── BasketStats.sln
```

## Key Dependencies

### API (`BasketStats.API`)
- `MediatR` — CQS dispatcher
- `Swashbuckle.AspNetCore` — Swagger UI with JWT Bearer support
- `Serilog.AspNetCore` — Structured request logging
- `Microsoft.AspNetCore.Authentication.JwtBearer` — Keycloak JWT validation

### Application (`BasketStats.Application`)
- `MediatR` — Command/query handler registration
- `FluentValidation` — Input validation (planned)

### Infrastructure (`BasketStats.Infrastructure`)
- `Google.Cloud.Firestore` — Firestore client
- `Google.Cloud.Storage.V1` — GCS client

### Tests
- `xUnit` + `Moq` — Unit test framework and mocking

## Local Development Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop

### 1. Start infrastructure

```bash
docker-compose up -d
```

This starts:
| Service | URL | Purpose |
|---|---|---|
| Keycloak | http://localhost:8080 | Auth (realm auto-imported) |
| Firestore Emulator | http://localhost:8080 (gRPC 8081) | Database |
| GCS Emulator | http://localhost:4443 | File storage |

### 2. Run the API

```bash
cd src/BasketStats.API
dotnet run
```

Available at:
- API: `http://localhost:5273`
- Swagger UI: `http://localhost:5273/swagger`

### 3. Run tests

```bash
# All unit tests
dotnet test tests/Unit/BasketStats.Domain.Tests/BasketStats.Domain.Tests.csproj
dotnet test tests/Unit/BasketStats.Application.Tests/BasketStats.Application.Tests.csproj
dotnet test tests/Unit/BasketStats.API.Tests/BasketStats.API.Tests.csproj
dotnet test tests/Unit/BasketStats.Infrastructure.Tests/BasketStats.Infrastructure.Tests.csproj

# Integration tests (requires docker-compose up)
dotnet test tests/Integration/BasketStats.Integration.Tests/BasketStats.Integration.Tests.csproj
```

## API Endpoints

### Authentication prerequisite

All write endpoints require a JWT Bearer token from Keycloak:
```
POST http://localhost:8080/realms/basket-stats/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=basket-stats-api&username=admin@basketstats.com&password=admin123&scope=openid
```

### Users

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/users/me` | ✅ | Self-register authenticated Keycloak user |

### Teams

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/teams` | ✅ | Create a team (requester becomes owner) |

### Matches

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/matches` | ✅ | Create match (homeTeamId, awayTeamId) |
| `GET` | `/api/matches` | ❌ | List matches (optional: `?teamId=&status=`) |
| `GET` | `/api/matches/{id}` | ❌ | Get match details |
| `PUT` | `/api/matches/{id}/start` | ✅ | Start match (Scheduled → Active) |
| `PUT` | `/api/matches/{id}/finish` | ✅ | Finish match (Active → Finished) |
| `POST` | `/api/matches/{id}/events` | ✅ | Add event to active match |

### Add Event — Request Body

```json
{
  "teamId": "string",
  "playerId": "string",
  "type": "Score | MissedShot | FreeThrow | Foul | Substitution",
  "periodNumber": "One | Two | Three | Four",
  "periodTimestamp": 60,

  "points": 2,
  "coordinatesX": 45.5,
  "coordinatesY": 30.0,

  "made": true,
  "foulType": "Personal | Technical | Flagrant",

  "playerFouledId": "string",
  "flagrant": false,

  "playerOutId": "string"
}
```

Event type rules:
- `Score` / `MissedShot` — requires `coordinatesX`, `coordinatesY`; Score requires `points` (2 or 3)
- `FreeThrow` — requires `made` (bool) + `foulType`; no coordinates
- `Foul` — requires `foulType`, `playerFouledId`, `flagrant`; no coordinates
- `Substitution` — requires `playerOutId`; no coordinates

## Configuration

### `appsettings.Development.json`

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/basket-stats",
    "Audience": "basket-stats-api"
  },
  "Firestore": {
    "ProjectId": "basket-stats-local",
    "EmulatorHost": "localhost:8081"
  }
}
```

### Test users (auto-created by Keycloak realm import)

| Email | Password | Roles |
|---|---|---|
| `admin@basketstats.com` | `admin123` | `admin`, `match-creator` |
| `creator@basketstats.com` | `creator123` | `match-creator` |
| `viewer@basketstats.com` | `viewer123` | — |

## Happy Path

See `src/BasketStats.API/BasketStats.API.http` for the complete 18-step
happy path (login → register user → create teams → create match → start → add events → finish).
