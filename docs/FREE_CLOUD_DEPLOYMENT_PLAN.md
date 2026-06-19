# FlowSpace Free Cloud Deployment Plan

This document outlines the strategy for deploying FlowSpace to a variety of free-tier cloud providers.

## 1. Cloud Architecture (Free Tier)

| Component | Provider | Tier | Why? |
| :--- | :--- | :--- | :--- |
| **Frontend** | Vercel | Hobby | Native Next.js support, edge optimized, easy deployments. |
| **Backend API** | Render / Koyeb | Free / Nano | Supports Dockerized ASP.NET Core apps. Render has good DX, Koyeb has high performance. |
| **Database** | Supabase | Free | Managed PostgreSQL 17, reliable, includes connection pooling. |
| **Redis** | Upstash | Free | Serverless Redis, perfect for low-latency caching and small payloads. |

---

## 2. Gap Analysis & Required Changes

### 2.1 Missing CORS Configuration (Critical)
The API currently lacks a CORS policy. When the frontend is hosted on `vercel.app` and the API on `onrender.com`, browsers will block all requests.
- **Action**: Implement a flexible CORS policy in `Program.cs` that reads allowed origins from environment variables.

### 2.2 Database Migration Strategy
The application does not currently run `Database.Migrate()` on startup.
- **Action**: For a low-complexity free-tier deployment, add a startup check to apply migrations automatically, or document the process for running them manually from a local machine targeting the Supabase connection string.

### 2.3 Playwright Dependencies
The API `Dockerfile` uses a Playwright base image. Some free-tier providers (like Render Free) have strict memory limits (512MB) that might struggle with full browser runtimes.
- **Action**: Monitor memory usage. If rendering fails, consider moving the Export Worker to a separate worker service or optimizing the headless browser startup flags.

---

## 3. Environment Variables Mapping

### 3.1 Backend (Render / Koyeb)
| Variable | Value / Source |
| :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Provided by Supabase (Transaction mode). |
| `Redis__ConnectionString` | Provided by Upstash (SSL enabled). |
| `Jwt__Secret` | Generated 32+ char string. |
| `Gemini__ApiKey` | Your Google AI Studio API Key. |
| `INVITE_CODE` | The secret code required for user registration. |
| `FrontendUrl` | `https://flowspace.vercel.app` |
| `AllowedOrigins` | `https://flowspace.vercel.app` (For CORS) |

### 3.2 Frontend (Vercel)
| Variable | Value / Source |
| :--- | :--- |
| `NEXT_PUBLIC_API_URL` | `https://flowspace-api.onrender.com/api/v1` |
| `NEXT_PUBLIC_HUB_URL` | `https://flowspace-api.onrender.com/hubs/collaboration` |

---

## 4. Deployment Steps

### Step 1: Provision Infrastructure
1. Create a **Supabase** project and copy the PostgreSQL connection string.
2. Create an **Upstash** Redis instance and copy the connection string.
3. Ensure you have your **Gemini API Key** ready.

### Step 2: Deploy Backend (Render)
1. Connect your GitHub repository.
2. Select the `apps/api` root directory.
3. Select **Docker** as the runtime.
4. Inject the environment variables listed in Section 3.1.
5. Deploy and wait for the `/health` check to pass.

### Step 3: Deploy Frontend (Vercel)
1. Connect your GitHub repository.
2. Select the `apps/web` root directory.
3. Vercel will automatically detect Next.js.
4. Inject the environment variables listed in Section 3.2.
5. Deploy.

---

## 5. Post-Deployment Verification
1. Access the Vercel URL.
2. Verify the Landing Page loads.
3. Test Registration/Login (Verifies API + DB + JWT).
4. Create a Board and draw (Verifies SignalR + Redis).
5. Trigger an AI generation (Verifies Gemini integration).
6. Perform an export (Verifies Playwright rendering).
