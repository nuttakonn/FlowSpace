# FlowSpace Backend Production Deployment Guide

This document details how to deploy the FlowSpace ASP.NET Core API to production cloud environments like **Render** or **Koyeb**.

## 1. Containerization (`Dockerfile`)

The backend is fully containerized using a multi-stage `Dockerfile` located in the project root.

- **Build Stage**: Uses the official `mcr.microsoft.com/dotnet/sdk:10.0` image to restore dependencies and publish the application via `dotnet publish`.
- **Runtime Stage**: Uses `mcr.microsoft.com/playwright/dotnet:v1.44.0-jammy`. This specialized image includes all browser binaries (Chromium, WebKit) required for the headless **Canvas Export Worker**.
- **Port**: The application listens on port **8080** by default.

---

## 2. Infrastructure as Code (`render.yaml`)

For **Render**, a Blueprint specification is provided. It automates the provisioning of the web service with correct health checks and environment mappings.

### How to use with Render:
1. Push your code to GitHub.
2. In Render dashboard, select "Blueprints".
3. Connect your repository. Render will automatically detect `render.yaml`.

---

## 3. Koyeb Deployment Configuration

Koyeb is a high-performance alternative to Render.

### Deployment Steps:
1. **Create Web Service**: Connect your GitHub repo.
2. **Build Settings**:
   - **Build Strategy**: Docker.
   - **Docker Context**: `.` (Root).
   - **Dockerfile Path**: `apps/api/Dockerfile`.
3. **App Settings**:
   - **Port**: 8080.
   - **Instance Type**: Nano or Micro (Note: If PDF exports fail, upgrade to Small to ensure enough RAM for Playwright).
4. **Health Check**:
   - **Path**: `/health`.
   - **Protocol**: HTTP.

---

## 4. Environment Variables

| Variable | Requirement | Description |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | **Required** | Set to `Cloud` for production settings. |
| `ConnectionStrings__DefaultConnection` | **Required** | PostgreSQL connection string (e.g. from Supabase). |
| `REDIS_URL` | **Required** | Redis/Upstash connection URL (`rediss://...`). |
| `Jwt__Secret` | **Required** | Secure 32+ character key for JWT signing. |
| `Gemini__ApiKey` | **Required** | Google AI Studio API Key. |
| `AllowedOrigins` | **Required** | Comma-separated frontend URLs (e.g. `https://flowspace.vercel.app`). |
| `ApplyMigrationsOnStartup` | Optional | Set to `true` to automatically update DB schema. |

---

## 5. Post-Deployment Verification

1. **Liveness Check**: Verify `GET /health` returns `200 OK` and `Healthy`.
2. **Logs Audit**: Monitor the console logs. You should see "ApplyMigrationsOnStartup: true" followed by "Migrations applied successfully" on first boot.
3. **SignalR Verification**: Ensure that the log contains "SignalR: Using Redis backplane" to confirm multi-instance scaling is operational.
