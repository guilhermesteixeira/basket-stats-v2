# Basket Stats - Clean Architecture & CQS Design

## Architecture Overview

This project follows **Clean Architecture** principles with **CQS (Command Query Separation)** pattern to ensure maintainability, testability, and scalability.

```
┌─────────────────────────────────────────┐
│   Presentation Layer (REST API)         │
│   Controllers | DTOs | Middleware       │
└────────────────┬────────────────────────┘
                 │
                 ↓ Uses MediatR
┌─────────────────────────────────────────┐
│   Application Layer (CQS)               │
│   Commands | Queries | Handlers         │
│   Business Logic Orchestration          │
└────────────────┬────────────────────────┘
                 │
            Depends on
                 ↓
┌─────────────────────────────────────────┐
│   Domain Layer (Business Rules)         │
│   Entities | Value Objects              │
│   Repository Interfaces | Rules         │
└─────────────────────────────────────────┘
                 ↑
          Implemented by
                 │
┌─────────────────────────────────────────┐
│   Infrastructure Layer                  │
│   Repositories | Data Access            │
│   External Services | Adapters          │
└─────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (`BasketStats.Domain`)

**Purpose**: Contains core business rules and domain models. Zero external dependencies.

**Contents**:
- **Entities**: Match, Event, User, Team
  - Objects with identity
  - Encapsulate business logic
  - Can only be instantiated when valid
  
- **Value Objects**: MatchId, EventId, Period, Coordinates, Score
  - Immutable objects without identity
  - Ensure valid states at creation
  
- **Repository Interfaces**: 
  - `IMatchRepository`
  - `IEventRepository`
  - `IUserRepository`
  - Defined but not implemented here
  
- **Domain Services** (if needed):
  - Pure business logic functions
  - Coordinate between entities

**Example**:
```csharp
public class Match
{
    public MatchId Id { get; private set; }
    public TeamId HomeTeamId { get; private set; }
    public TeamId AwayTeamId { get; private set; }
    public List<Event> Events { get; private set; }
    
    public void AddEvent(Event @event)
    {
        if (Status != MatchStatus.Active)
            throw new InvalidOperationException("Cannot add event to non-active match");
        
        Events.Add(@event);
    }
}
```

### 2. Application Layer (`BasketStats.Application`)

**Purpose**: Implements use cases and orchestrates domain logic. Contains CQS pattern.

**Contents**:

- **Commands**: Write operations
  - `CreateMatchCommand`: Initiate new match
  - `AddEventCommand`: Record match event
  - `UpdateMatchStatusCommand`: Change match state
  
- **Queries**: Read operations
  - `GetMatchQuery`: Fetch single match
  - `ListMatchesQuery`: Fetch multiple matches
  - `GetPlayerStatsQuery`: Calculate player statistics
  
- **Handlers**: Implement commands and queries
  - `CreateMatchCommandHandler`: Uses domain logic
  - `GetMatchQueryHandler`: Queries repository
  
- **DTOs**: Data transfer objects
  - `CreateMatchDto`: API request contract
  - `MatchDto`: API response contract
  
- **Application Services**: Orchestration
  - Handle cross-cutting concerns
  - Coordinate multiple domain operations

**Example**:
```csharp
public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, MatchDto>
{
    private readonly IMatchRepository _repository;
    
    public async Task<MatchDto> Handle(CreateMatchCommand request, CancellationToken ct)
    {
        var match = Match.Create(request.HomeTeamId, request.AwayTeamId);
        await _repository.SaveAsync(match, ct);
        return MapToDto(match);
    }
}
```

### 3. Infrastructure Layer (`BasketStats.Infrastructure`)

**Purpose**: Implements persistence and external integrations.

**Contents**:

- **Repository Implementations**:
  - Firestore-based repositories
  - Implement domain repository interfaces
  - Data access logic
  
- **Data Mappers**:
  - Domain entity ↔ Firestore document conversion
  - Ensure database independence
  
- **External Service Adapters**:
  - Keycloak authentication client
  - Cloud Storage adapter
  
- **Configuration**:
  - Firestore collection setup
  - Index definitions
  - Connection strings

**Example**:
```csharp
public class FirestoreMatchRepository : IMatchRepository
{
    private readonly CollectionReference _collection;
    
    public async Task SaveAsync(Match match, CancellationToken ct)
    {
        var doc = MapToDocument(match);
        await _collection.Document(match.Id.Value).SetAsync(doc, cancellationToken: ct);
    }
}
```

### 4. Presentation Layer (`BasketStats.API`)

**Purpose**: HTTP interface and API contracts.

**Contents**:

- **Controllers**: Thin controllers
  - Receive HTTP requests
  - Delegate to MediatR
  - Return HTTP responses
  
- **DTOs**: API contracts
  - Request models
  - Response models
  - Differ from application DTOs (if needed)
  
- **Middleware**: Cross-cutting concerns
  - Authentication
  - Authorization
  - Error handling
  - CORS
  
- **Configuration** (Program.cs):
  - Service registration
  - Dependency injection
  - Middleware pipeline

**Example**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request)
    {
        var command = new CreateMatchCommand(request.HomeTeamId, request.AwayTeamId);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## CQS Pattern

**Command Query Separation** divides operations into:

### Commands (Write Operations)
- Modify state
- Return success/failure (not data)
- Exception-driven error handling

Example commands:
```
CreateMatchCommand → CreateMatchCommandHandler → void/MatchId
AddEventCommand → AddEventCommandHandler → void
UpdateMatchStatusCommand → UpdateMatchStatusCommandHandler → void
```

### Queries (Read Operations)
- Don't modify state
- Return data
- Never throw exceptions (return null/empty)

Example queries:
```
GetMatchQuery → GetMatchQueryHandler → MatchDto
ListMatchesQuery → ListMatchesQueryHandler → List<MatchDto>
GetPlayerStatsQuery → GetPlayerStatsQueryHandler → PlayerStatsDto
```

## Dependency Injection

All services registered in `Program.cs`:

```csharp
// Domain - no registration (interfaces defined)
services.AddDomain();

// Application - MediatR handlers and DTOs
services.AddApplicationServices();
services.AddMediatR(typeof(Application.AssemblyReference).Assembly);

// Infrastructure - Firestore repositories
services.AddInfrastructureServices(configuration);

// Presentation - API configuration
services.AddApiServices();
```

## Testing Strategy

### Unit Tests

| Project | Tests | Focus |
|---|---|---|
| `BasketStats.Domain.Tests` | 88 | Entity creation/validation, business rules, value object equality |
| `BasketStats.Application.Tests` | 51 | Command/query handlers (mocked repositories) |
| `BasketStats.API.Tests` | 14 | Controller layer, request/response mapping |
| `BasketStats.Infrastructure.Tests` | 19 | Firestore mapper conversions |

### Integration Tests (`BasketStats.Integration.Tests`)
- Repository implementations against Firestore emulator
- Requires `docker-compose up`

### Frontend Tests (planned — `frontend/`)
- **Unit/Component**: Vitest + React Testing Library
- **E2E**: Playwright (optional future addition)

See `TEST_SCENARIOS.md` for full scenario catalogue.

## Data Flow Example: Create Match

```
1. POST /api/matches {"homeTeamId": "...", "awayTeamId": "..."}
                     ↓
2. MatchesController.Create(CreateMatchRequest)
                     ↓
3. MediatR.Send(new CreateMatchCommand(...))
                     ↓
4. CreateMatchCommandHandler.Handle()
   - Calls Match.Create() (domain logic)
   - Calls _repository.SaveAsync() (infrastructure)
   - Returns MatchDto (application)
                     ↓
5. Controller returns CreatedAtAction(201)
```

## Best Practices

1. **Domain Layer**
   - Keep pure business logic
   - Use value objects for validation
   - No external dependencies

2. **Application Layer**
   - Thin orchestration only
   - Let domain handle complexity
   - Use interfaces from domain

3. **Infrastructure Layer**
   - Implement interfaces (don't create new ones)
   - Keep Firestore logic isolated
   - Use mappers to decouple entities

4. **Presentation Layer**
   - Controllers are thin
   - Delegate to application layer
   - Don't bypass MediatR

5. **General**
   - Depend on abstractions (interfaces)
   - Never skip layers
   - Keep layers independent

## File Organization

```
src/BasketStats.Domain/
├── Entities/
│   ├── Match.cs
│   ├── Event.cs
│   └── User.cs
├── ValueObjects/
│   ├── MatchId.cs
│   ├── Coordinates.cs
│   └── Period.cs
├── Abstractions/
│   ├── IMatchRepository.cs
│   ├── IEventRepository.cs
│   └── Entity.cs
└── Exceptions/
    └── [Domain-specific exceptions]

src/BasketStats.Application/
├── Commands/
│   ├── CreateMatch/
│   │   ├── CreateMatchCommand.cs
│   │   └── CreateMatchCommandHandler.cs
│   └── AddEvent/
├── Queries/
│   ├── GetMatch/
│   │   ├── GetMatchQuery.cs
│   │   └── GetMatchQueryHandler.cs
│   └── ListMatches/
├── DTOs/
│   ├── MatchDto.cs
│   └── EventDto.cs
└── Mappings/
    └── [AutoMapper profiles]

src/BasketStats.Infrastructure/
├── Repositories/
│   └── FirestoreMatchRepository.cs
├── Mappers/
│   └── MatchMapper.cs
├── Authentication/
│   └── KeycloakService.cs
└── Configuration/
    └── FirestoreConfiguration.cs

src/BasketStats.API/
├── Controllers/
│   ├── MatchesController.cs
│   └── EventsController.cs
├── Requests/
│   └── CreateMatchRequest.cs
├── Responses/
│   └── MatchResponse.cs
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
└── Program.cs
```

## References

- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQS Pattern: https://martinfowler.com/bliki/CommandQuerySeparation.html
- Domain-Driven Design: https://www.domainlanguage.com/ddd/

---

## Frontend Architecture

> See `FRONTEND.md` for the full frontend setup and plan.

The React frontend is a **separate application** (`frontend/`) that communicates with the backend exclusively via REST API. It has no knowledge of the backend's internal structure.

```
┌──────────────────────────────────────────┐
│   React Frontend (Vite + React 19)        │
│   Tailwind CSS · TanStack Query · Zustand │
│   Keycloak JS · React Router              │
└──────────────────┬───────────────────────┘
                   │ HTTP / REST (JWT Bearer)
                   ↓
┌──────────────────────────────────────────┐
│   BasketStats.API  (ASP.NET Core 10)      │
│   Swagger: http://localhost:5273/swagger  │
└──────────────────────────────────────────┘
```

### Screens (MVP)

| Screen | Route | Description |
|---|---|---|
| Login | `/login` | Keycloak-delegated authentication |
| Team Registration | `/teams/new` | Create a team (authenticated) |
| Match List | `/` | List all matches with status filter |
| Live Event Recording | `/matches/:id/live` | Add events in real-time (owner/opponent) |
| Match Statistics | `/matches/:id/stats` | Score timeline, foul counts, player heat map |

### Authentication Flow

```
1. User opens app → Keycloak JS checks for valid token
2. No token → redirect to Keycloak login page
3. Keycloak issues JWT → redirect back with code
4. Frontend calls POST /api/users/me (auto-register)
5. All API calls include Authorization: Bearer <token>
6. Token refresh handled automatically by Keycloak JS adapter
```

## Keycloak Authentication & Authorization

### Overview

Keycloak is the **Identity and Access Management (IAM)** system used for:
- **Authentication**: User login/logout via OpenID Connect
- **Authorization**: Role-based access control (RBAC)
- **User Management**: Profile, password, account settings
- **Social Login**: Google OAuth integration
- **Token Management**: JWT issued for API requests

### Architecture Integration

```
┌──────────────────────────────────────────────┐
│   Frontend / Client Application              │
└────────────────┬─────────────────────────────┘
                 │
                 ↓ 1. Login request
┌──────────────────────────────────────────────┐
│   Keycloak Server (http://localhost:8080)    │
│   - Authentication endpoint                  │
│   - Token endpoint                           │
│   - User info endpoint                       │
└────────────────┬─────────────────────────────┘
                 │
                 ↓ 2. JWT Token issued
┌──────────────────────────────────────────────┐
│   BasketStats.API                            │
│   - Validate JWT signature                   │
│   - Check token expiry                       │
│   - Extract claims (roles, user info)        │
└────────────────┬─────────────────────────────┘
                 │
                 ↓ 3. Create authenticated principal
┌──────────────────────────────────────────────┐
│   Application (MediatR Commands/Queries)     │
│   - Access ClaimsPrincipal                   │
│   - Check authorization rules                │
└──────────────────────────────────────────────┘
```

### Authentication Flow

#### 1. User Login (OpenID Connect)

```
Frontend                    Keycloak                    API
   │                          │                          │
   │──── Login ──────────────→│                          │
   │                          │                          │
   │                    [Verify credentials]             │
   │                          │                          │
   │←──── JWT Token ──────────│                          │
   │                          │                          │
   │────────── API Request (with JWT) ─────────────────→│
   │                                                     │
   │←────────────── Response ────────────────────────────│
```

#### 2. JWT Token Validation (in API)

```
Request arrives with Bearer token in Authorization header
              ↓
JwtBearerHandler validates:
  - Token signature (using Keycloak public key)
  - Token expiry
  - Token claims
              ↓
Creates ClaimsPrincipal from token claims
              ↓
Middleware adds to HttpContext.User
              ↓
Available in Controllers and MediatR handlers
```

### Configuration

#### Keycloak Setup (Docker)
```yaml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:latest
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    command:
      - start-dev
```

**Access**: http://localhost:8080/admin (user: `admin`, password: `admin`)

#### Create Realm in Keycloak
1. Log in to Keycloak admin console
2. Create realm: `basket-stats`
3. Create client:
   - Client ID: `basket-stats-api`
   - Access Type: `confidential`
   - Valid Redirect URI: `http://localhost:5000/login/oauth2/code/keycloak`
   - Save and copy `Client Secret`

#### Application Configuration (appsettings.json)

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Authority": "http://localhost:8080/realms/basket-stats",
        "ValidateAudience": false,
        "Audience": "basket-stats-api"
      }
    }
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/basket-stats",
    "ClientId": "basket-stats-api",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

#### Program.cs Setup

```csharp
// Add authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Authentication:Schemes:Bearer:Authority"];
        options.Authority = authority;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false,
            ValidIssuer = authority,
            ValidateIssuerSigningKey = true
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Use authentication middleware
app.UseAuthentication();
app.UseAuthorization();
```

### Claims & Roles

#### Claims from Keycloak JWT

```json
{
  "sub": "user-id-uuid",
  "email": "user@example.com",
  "name": "John Doe",
  "roles": ["match-creator", "viewer", "admin"],
  "iat": 1234567890,
  "exp": 1234571490
}
```

#### Role-Based Authorization

```csharp
// In controller
[Authorize(Roles = "match-creator")]
[HttpPost]
public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
{
    // Only users with 'match-creator' role can access
}

// In MediatR handler
public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, MatchDto>
{
    private readonly ICurrentUserService _currentUser;
    
    public async Task<MatchDto> Handle(CreateMatchCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsInRole("match-creator"))
            throw new UnauthorizedException("Not authorized to create matches");
        
        // Business logic
    }
}
```

### Infrastructure Layer - Authentication Services

#### ICurrentUserService Interface (Domain)

```csharp
namespace BasketStats.Domain.Abstractions;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    List<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

#### KeycloakUserService Implementation (Infrastructure)

```csharp
namespace BasketStats.Infrastructure.Authentication;

public class KeycloakUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public string UserId => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    
    public string Email => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    
    public List<string> Roles => _httpContextAccessor.HttpContext?.User
        .FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList() 
        ?? new List<string>();
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public bool IsInRole(string role) => Roles.Contains(role);
}
```

#### KeycloakClient (Infrastructure)

```csharp
namespace BasketStats.Infrastructure.Authentication;

public class KeycloakClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    
    // Token introspection
    public async Task<bool> IsTokenValidAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/protocol/openid-connect/introspect")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("client_id", _settings.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret)
            })
        };
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
    
    // User info endpoint
    public async Task<KeycloakUserInfo> GetUserInfoAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("/protocol/openid-connect/userinfo");
        // Parse and return user info
    }
}
```

### Application Layer - Authorization Queries

```csharp
// Query to check if user can create match
public record CanUserCreateMatchQuery(string UserId) : IRequest<bool>;

public class CanUserCreateMatchQueryHandler : IRequestHandler<CanUserCreateMatchQuery, bool>
{
    private readonly ICurrentUserService _currentUser;
    
    public async Task<bool> Handle(CanUserCreateMatchQuery request, CancellationToken ct)
    {
        return _currentUser.IsInRole("match-creator");
    }
}
```

### Google OAuth Integration

#### Setup in Keycloak

1. Go to realm → Identity Providers → Add → Google
2. Enter Google OAuth credentials (from Google Cloud Console)
3. Set redirect URI: `http://localhost:8080/realms/basket-stats/broker/google/endpoint`

#### Frontend Integration

```javascript
// Redirect to Google login via Keycloak
window.location.href = 'http://localhost:8080/realms/basket-stats/protocol/openid-connect/auth?client_id=basket-stats-api&response_type=code&scope=openid+profile+email&redirect_uri=http://localhost:3000/callback'
```

### Security Best Practices

1. **Token Storage**: Store JWT in httpOnly cookies (not localStorage)
2. **Token Refresh**: Use refresh tokens for long-lived sessions
3. **HTTPS**: Always use HTTPS in production
4. **Scope Limiting**: Request minimal scopes from Keycloak
5. **Token Validation**: Always validate tokens server-side
6. **Role Checking**: Never trust roles from frontend; always verify server-side
7. **CORS**: Restrict CORS origins to trusted domains
8. **Rate Limiting**: Implement rate limiting on token endpoints

### Testing Authentication

#### Mock Current User Service

```csharp
[Fact]
public async Task CreateMatch_WithValidUser_Succeeds()
{
    // Arrange
    var mockUserService = new Mock<ICurrentUserService>();
    mockUserService.Setup(x => x.UserId).Returns("user-123");
    mockUserService.Setup(x => x.IsInRole("match-creator")).Returns(true);
    
    var handler = new CreateMatchCommandHandler(mockUserService.Object, _repository);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task CreateMatch_WithoutRole_Throws()
{
    // Arrange
    var mockUserService = new Mock<ICurrentUserService>();
    mockUserService.Setup(x => x.IsInRole("match-creator")).Returns(false);
    
    var handler = new CreateMatchCommandHandler(mockUserService.Object, _repository);
    
    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedException>(() => 
        handler.Handle(command, CancellationToken.None));
}
```

### Useful Keycloak Endpoints

```
# Admin Console
GET /admin

# Realm info
GET /realms/basket-stats

# Token endpoint (for service-to-service calls)
POST /realms/basket-stats/protocol/openid-connect/token

# User info
GET /realms/basket-stats/protocol/openid-connect/userinfo

# Logout
GET /realms/basket-stats/protocol/openid-connect/logout

# Public keys (for token validation)
GET /realms/basket-stats/protocol/openid-connect/certs
```

### Environment Variables (Docker)

```env
# .env.local
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
KEYCLOAK_REALM=basket-stats
KEYCLOAK_CLIENT_ID=basket-stats-api
KEYCLOAK_CLIENT_SECRET=<generated-secret>

# Application
KEYCLOAK_AUTHORITY=http://localhost:8080/realms/basket-stats
JWT_VALID_AUDIENCE=basket-stats-api
```
