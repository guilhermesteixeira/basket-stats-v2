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

## Troubleshooting

**Services won't start:**
- Check port availability: `lsof -i :8080` (example for port 8080)
- Ensure Docker is running: `docker ps`

**Firestore connection refused:**
- Wait 10-15 seconds for emulator to start
- Check: `curl http://localhost:8081`

**Keycloak admin console blank:**
- Clear browser cache or use incognito mode
- Wait 30s for startup
