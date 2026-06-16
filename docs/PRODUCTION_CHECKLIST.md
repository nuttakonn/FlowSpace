# FlowSpace Production Readiness Checklist

This document outlines the critical operational requirements, configurations, and strategies to ensure FlowSpace is ready for a production deployment.

## 1. Environment & Configuration
- [x] **Secure Secrets Management**: Database credentials, JWT secrets, and Gemini API keys are loaded via Environment Variables or Docker Secrets (`/run/secrets`). No secrets are hardcoded.
- [x] **Environment Profiles**: Added `appsettings.Production.json` to configure structured JSON logging for the backend.
- [x] **Next.js Standalone**: Frontend is configured to build in standalone mode for optimized Docker deployment (configured in `next.config.ts`).

## 2. Scalability & Resilience
- [x] **Rate Limiting**: Configured global rate limiting on the API (100 requests / 10 seconds per client) to prevent DDoS attacks and API abuse.
- [x] **Health Checks**: Implemented ASP.NET Core Health Checks at `/health`.
    - Includes `AddDbContextCheck` to ensure database connectivity is actively monitored.
    - Used by Kubernetes/Docker Swarm for liveness and readiness probes.
- [x] **Background Workers**: Heavy tasks like Canvas Export (Playwright) are decoupled using BackgroundServices and Channels to prevent blocking the main API thread.

## 3. Database & Storage
- [x] **PostgreSQL Tuned**: Utilizing compound filtered indexes (e.g., `WHERE is_deleted = false`) to handle 100k+ nodes efficiently.
- [x] **Migrations**: All Entity Framework Core migrations are generated and tested. Ensure migrations are run sequentially during CI/CD before app startup.
- [ ] **Backup Strategy (Action Required)**: 
    - **Database**: Configure daily full backups and continuous WAL (Write-Ahead Logging) archiving using tools like `pgBackRest` or managed provider backups (AWS RDS/Azure PostgreSQL).
    - **MinIO/S3**: Configure cross-region replication or versioning for stored images and export artifacts.

## 4. Security
- [x] **Authentication**: JWT validation with strict audience/issuer checks. Refresh token rotation is implemented.
- [x] **HTTPS Redirection**: Enforced via `app.UseHttpsRedirection()`.
- [x] **RBAC Enforcement**: All mutating endpoints and SignalR Hub connections strictly validate Board/Workspace roles via `IPermissionService`.
- [x] **XML/XXE Protection**: The draw.io XML importer utilizes a custom `XmlReader` with DTD processing strictly prohibited.

## 5. Observability
- [x] **Structured Logging**: Serilog is configured to output `CompactJsonFormatter` logs to `Console` in production. This allows seamless ingestion into Elasticsearch, Datadog, or CloudWatch.
- [x] **Global Exception Handling**: Custom `ExceptionHandlingMiddleware` catches unhandled exceptions, logs them with stack traces, and returns standardized RFC 7807 Problem Details to the client.
- [ ] **APM Integration (Action Required)**: Connect OpenTelemetry, Datadog, or Application Insights for distributed tracing between Next.js, the ASP.NET Core API, and PostgreSQL.

## 6. Frontend Production Polish
- [x] **Bundle Optimization**: Ensure all components and icons are tree-shaken.
- [x] **Image Optimization**: Next.js Image component utilized where applicable.

---
**Status**: The application codebase meets production standards. The remaining actions revolve around infrastructure provisioning (Database Backups, APM connection) within the target hosting environment (e.g., AWS, Azure, or Kubernetes).
