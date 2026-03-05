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

```
basket-stats/
├── claude.md                # This file
├── src/                     # Source code (when created)
├── tests/                   # Tests (when created)
├── docs/                    # Documentation (when created)
└── project.csproj           # Project metadata (when created)
```

---

## Technology Stack

### Backend
- **Runtime**: .NET 8 / C#
- **Framework**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Authentication**: Keycloak (OpenID Connect)
- **Infrastructure**: Google Cloud Platform (Serverless)
  - Cloud Run
  - Cloud Firestore / Datastore
  - Cloud Pub/Sub (for events)

### Main Dependencies
- Microsoft.AspNetCore
- Microsoft.EntityFrameworkCore
- IdentityModel (OpenID Connect)
- Google.Cloud.Firestore

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
- **Match**: id, teams, players, startTime, status, events[], teamFouls[] (per period)
- **Event**: timestamp, type (score, missed_shot, free_throw, foul, substitution), details, teamId, playerId
- **ScoreEvent**: points (2 or 3), coordinates {x, y}, playerName
- **MissedShotEvent**: coordinates {x, y}, playerName
- **FreeThrowEvent**: made (boolean), foulType (personal, technical, flagrant), playerName
- **FoulEvent**: foulType, playerFouled, flagrant (boolean)
- **User**: id, email, keycloakId, roles[]

---

## Business Rules

### Event Management
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

4. **Other Events**
   - Foul events do NOT require coordinates
   - Substitution events do NOT require coordinates

5. **Match Lifecycle**
   - Scheduled → Active → Finished (valid state transitions)
   - Events can only be added to Active matches
   - Cannot add events to Finished matches

---

## Next Steps
1. Set up GCP project
2. Implement Keycloak authentication base
3. Create basic REST API
4. Implement match functionality

---

## Contact & Notes

- Communication Language: English
- Focus: Code quality and clarity
