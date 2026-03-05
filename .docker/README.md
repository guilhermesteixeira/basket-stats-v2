# Docker Setup for Basket Stats

## Services

This docker-compose file sets up all development dependencies:

### 1. **Keycloak** (Port 8080)
- Authentication & Authorization server
- OpenID Connect provider
- Admin console: http://localhost:8080/admin
- Default credentials: `admin` / `admin`

### 2. **Firestore Emulator** (Port 8081)
- Local Google Cloud Firestore database
- Zero data persistence (in-memory)
- Perfect for development & testing
- Connection string: `http://localhost:8081`

### 3. **Google Cloud Storage Emulator** (Port 4443)
- Fake GCS server for local storage
- HTTP mode (not HTTPS)
- In-memory backend
- Endpoint: `http://localhost:4443`

## Getting Started

### Start all services
```bash
docker-compose up -d
```

### Stop all services
```bash
docker-compose down
```

### View logs
```bash
docker-compose logs -f [service-name]
```

### Remove volumes (reset data)
```bash
docker-compose down -v
```

## Environment Variables for .NET App

```env
# Keycloak
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=master
KEYCLOAK_CLIENT_ID=basket-stats
KEYCLOAK_CLIENT_SECRET=<generated-secret>

# Firestore
GOOGLE_CLOUD_PROJECT=basket-stats-dev
FIRESTORE_EMULATOR_HOST=localhost:8081

# GCS
GCS_ENDPOINT=http://localhost:4443
GCS_PROJECT_ID=basket-stats-dev

# Database
DATABASE_URL=postgresql://basket_user:basket_password@localhost:5432/basket_stats
```

## Health Checks

All services include health checks. Verify they're running:

```bash
docker-compose ps
```

Expected status: `healthy` for all services

## Useful Commands

### Create Firestore database (interactive)
```bash
gcloud firestore databases create --database=default --location=us-central1
```

### Reset Firestore data
```bash
docker-compose down -v && docker-compose up -d firestore-emulator
```

### Access Keycloak Admin
1. Navigate to http://localhost:8080/admin
2. Login with `admin` / `admin`
3. Configure realm and clients

### Configure Google OAuth (Optional but Recommended)

#### Step 1: Create Google OAuth Credentials
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project (or use existing)
3. Enable OAuth 2.0
4. Create OAuth 2.0 Client ID (Web application):
   - Authorized redirect URIs: `http://localhost:8080/realms/master/broker/google/endpoint`
5. Copy Client ID and Client Secret

#### Step 2: Add Google as Identity Provider in Keycloak
1. Go to Admin Console: http://localhost:8080/admin
2. Select realm (master)
3. Go to **Identity Providers** → Add provider → Google
4. Paste Google Client ID and Client Secret
5. Set Display name: "Google"
6. Save

#### Step 3: Configure OAuth Scope (Optional)
Default scopes: `openid profile email`

#### Step 4: Update Application Configuration
Add to `.env`:
```env
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
KEYCLOAK_GOOGLE_PROVIDER_URL=http://localhost:8080/realms/master/broker/google
```

#### Step 5: Test Login Flow
- Visit your app login page
- Should show "Login with Google" option
- Click and authenticate with Google account
- Should redirect back to app

#### Keycloak + Google Flow
```
User → App → Keycloak → Google → Keycloak → App
                         ↓
                    OAuth token
```

**Services won't start:**
- Check port availability: `lsof -i :8080` (example for port 8080)
- Ensure Docker is running: `docker ps`

**Firestore connection refused:**
- Wait 10-15 seconds for emulator to start
- Check: `curl http://localhost:8081`

**Keycloak admin console blank:**
- Clear browser cache or use incognito mode
- Wait 30s for startup
