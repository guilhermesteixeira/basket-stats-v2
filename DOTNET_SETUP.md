# .NET 9 ASP.NET Core Project Setup

## Project Structure

```
src/
├── BasketStats.API/
│   ├── Controllers/
│   │   ├── MatchesController.cs    # Match CRUD operations
│   │   └── EventsController.cs     # Event management (score, foul, etc)
│   ├── Models/
│   │   ├── Match.cs               # Match domain model
│   │   └── User.cs                # User model
│   ├── Services/                   # Business logic (to be implemented)
│   ├── Data/                       # Firestore repositories (to be implemented)
│   ├── appsettings.json           # Configuration
│   ├── appsettings.Development.json
│   ├── Program.cs                  # Application startup
│   └── BasketStats.API.csproj     # Project file

tests/
├── BasketStats.API.Tests/         # Unit tests (to be implemented)
```

## Key Dependencies

### Authentication
- `Microsoft.AspNetCore.Authentication.OpenIdConnect` - Keycloak integration
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT token validation

### Data
- `Google.Cloud.Firestore` - NoSQL database
- `Google.Cloud.Storage.V1` - Object storage
- `Microsoft.EntityFrameworkCore` - ORM

### Validation & Quality
- `FluentValidation` - Model validation
- `Serilog` - Structured logging
- `xunit` - Testing framework
- `Moq` - Mocking library

## Building the Project

### Prerequisites
- .NET 9 SDK
- Docker (for dependencies)

### Setup

1. **Start dependencies**
   ```bash
   docker-compose up -d
   ```

2. **Restore packages** (once .NET SDK is installed)
   ```bash
   cd src/BasketStats.API
   dotnet restore
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

4. **API will be available at**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

## Configuration

### Environment Variables

```bash
# Keycloak
KEYCLOAK_CLIENT_SECRET=your-secret

# Google Cloud
GOOGLE_APPLICATION_CREDENTIALS=/path/to/credentials.json (optional for GCS)
```

### appsettings.json

- Keycloak URL and realm configuration
- Firestore emulator host
- JWT audience and claims mapping
- CORS policy settings

## API Endpoints (To Be Implemented)

### Matches
- `GET /api/matches` - List matches (paginated)
- `GET /api/matches/{id}` - Get match details
- `POST /api/matches` - Create new match
- `PUT /api/matches/{id}` - Update match status
- `DELETE /api/matches/{id}` - Delete match (admin only)

### Events
- `GET /api/matches/{id}/events` - Get match events
- `POST /api/matches/{id}/events/score` - Add score event
- `POST /api/matches/{id}/events/missed-shot` - Add missed shot
- `POST /api/matches/{id}/events/free-throw` - Add free throw
- `POST /api/matches/{id}/events/foul` - Add foul
- `POST /api/matches/{id}/events/substitution` - Add substitution

## Next Steps

1. Implement Firestore repository layer
2. Add event ownership validation
3. Implement heat map generation service
4. Add comprehensive unit tests
5. Setup CI/CD pipeline
