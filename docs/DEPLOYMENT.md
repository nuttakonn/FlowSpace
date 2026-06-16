# FlowSpace Deployment Guide

This document provides instructions for deploying FlowSpace to a production environment using Docker and Docker Compose.

## Architecture

The production deployment consists of four core containers orchestrated within a private Docker bridge network:
1. **PostgreSQL 17**: The primary relational database for users, workspaces, boards, nodes, and templates.
2. **Redis 7**: The in-memory cache used for RBAC permission resolution and potentially SignalR backplane scaling.
3. **FlowSpace API**: The ASP.NET Core 10 backend. It runs on a specialized Playwright container image to support server-side rendering of Canvas exports.
4. **FlowSpace Web**: The Next.js 16 frontend running in standalone mode for optimized footprint and fast startup.

---

## Prerequisites

- A Linux server (Ubuntu 22.04+ recommended) with at least 4GB RAM and 2 vCPUs.
- [Docker Engine](https://docs.docker.com/engine/install/) installed.
- [Docker Compose](https://docs.docker.com/compose/install/) installed.
- A domain name pointing to your server's IP address (if setting up an Nginx reverse proxy).

---

## Step-by-Step Deployment

### 1. Clone the Repository
Clone the FlowSpace repository to your production server.
```bash
git clone https://github.com/your-org/flowspace.git
cd flowspace
```

### 2. Configure Environment Variables
Create a `.env` file in the root directory to securely configure your secrets.

```bash
cp .env.example .env
nano .env
```

**Required `.env` Configuration:**
```env
# Database Credentials
POSTGRES_USER=flowspace_admin
POSTGRES_PASSWORD=your_secure_db_password
POSTGRES_DB=flowspace_prod

# Redis Credentials
REDIS_PASSWORD=your_secure_redis_password

# Authentication Secrets (Must be > 32 chars)
JWT_SECRET=your_super_secret_cryptographic_key_here
JWT_ISSUER=https://api.yourdomain.com
JWT_AUDIENCE=https://app.yourdomain.com

# AI Integration
GEMINI_API_KEY=your_google_gemini_api_key

# Public URLs
FRONTEND_URL=https://app.yourdomain.com
NEXT_PUBLIC_API_URL=https://api.yourdomain.com/api/v1
NEXT_PUBLIC_HUB_URL=https://api.yourdomain.com/hubs/collaboration
```

### 3. Initialize Database Migrations
Before fully launching the application, you must apply the Entity Framework migrations to structure the PostgreSQL database.

*Note: In a standard deployment pipeline, migrations are often run via a separate job. Alternatively, ensure the API has logic to apply migrations on startup if acceptable for your environment.*

If running manually against the deployed Postgres instance:
```bash
# Start just the database
docker-compose -f docker-compose.production.yml up -d postgres
# (Wait for postgres to be healthy)

# Apply migrations from your local machine targeting the remote DB, 
# or configure the API container to run `dotnet ef database update` on entry.
```

### 4. Launch the Stack
Start the entire infrastructure in detached mode.

```bash
docker-compose -f docker-compose.production.yml up --build -d
```

Docker Compose will:
1. Start `postgres` and `redis`.
2. Wait for their health checks to pass.
3. Build and start the `api` container (downloading the Playwright runtime).
4. Wait for the API `/health` endpoint to pass.
5. Build and start the `web` container using Next.js standalone output.

### 5. Verify Deployment
Check the status of your containers:
```bash
docker-compose -f docker-compose.production.yml ps
```
You should see all four services marked as `Up (healthy)`.

---

## Production Recommendations

### Reverse Proxy & SSL (Nginx / Traefik / Caddy)
Do not expose the raw ports (`3000`, `5000`) directly to the internet. Use a reverse proxy like Nginx to terminate SSL/TLS (via Let's Encrypt) and route traffic to the respective containers.

**Important**: Ensure your reverse proxy is configured to support **WebSockets** for SignalR real-time collaboration.

*Nginx WebSocket Configuration Snippet:*
```nginx
location /hubs/collaboration {
    proxy_pass http://localhost:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```

### Backups
The `pgdata` Docker volume contains all your visual board data. Configure a daily cron job to back up this volume to an offsite location (e.g., AWS S3).
```bash
# Example pg_dump command
docker exec flowspace-postgres pg_dump -U flowspace_admin flowspace_prod > backup_$(date +%Y%m%d).sql
```
