# FlowSpace Cloud Security Configuration

This document outlines the security measures implemented for FlowSpace when deployed in a cloud environment (e.g., Render, Koyeb, Vercel, Supabase).

## 1. Transport Security (HTTPS)

- **HTTPS Redirection**: All non-HTTPS traffic is automatically redirected to HTTPS via `app.UseHttpsRedirection()`.
- **HSTS (HTTP Strict Transport Security)**: In non-development environments, HSTS is enabled with `Preload`, `IncludeSubDomains`, and a `MaxAge` of 365 days. This instructs browsers to always communicate with the API over HTTPS.
- **Forwarded Headers**: Configured to trust `X-Forwarded-For` and `X-Forwarded-Proto` headers. This is critical for cloud deployments where the application sits behind a load balancer/proxy (Render/Koyeb), ensuring the application can correctly identify the client's original protocol and IP address.

---

## 2. Cross-Origin Resource Sharing (CORS)

FlowSpace implements a dynamic **`CloudCorsPolicy`**:
- **Allowed Origins**: Reads from the `AllowedOrigins` environment variable. This should be set to your frontend's Vercel URL (e.g., `https://flowspace.vercel.app`).
- **Allowed Methods/Headers**: Allows all standard methods and headers required by the Next.js frontend and SignalR client.
- **Credentials**: Supports `AllowCredentials()`, which is mandatory for authenticated **SignalR** WebSocket connections.

---

## 3. Infrastructure Hardening (Rate Limiting)

Implemented using the native ASP.NET Core Rate Limiting middleware:
- **`GlobalLimit`**: Applied to all controllers. Limits clients to 100 requests every 10 seconds with a small queue.
- **`AuthLimit`**: Applied specifically to the `AuthController`. Limits registration, login, and token refresh attempts to 10 requests per minute with no queue. This provides critical protection against brute-force and credential-stuffing attacks.

---

## 4. Application Secrets

- **Environment Injection**: All sensitive credentials (DB passwords, JWT keys, AI API keys) are injected via Environment Variables.
- **Precedence**: The application is configured to prioritize Environment Variables and Docker Secrets over static JSON files, ensuring that production secrets are never accidentally overridden by default settings.

---

## 5. Persistence Security

- **Supabase SSL**: The application is configured to require SSL for all database connections to Supabase PostgreSQL 17.
- **Upstash TLS**: Redis connections utilize the `rediss://` protocol, ensuring all cached RBAC roles and SignalR backplane traffic are encrypted in transit.

---

## 6. Authentication Architecture

- **CSPRNG Tokens**: Public share links and refresh tokens are generated using a Cryptographically Secure Pseudo-Random Number Generator (`System.Security.Cryptography.RandomNumberGenerator`).
- **Hashed Refresh Tokens**: Refresh tokens are stored in the database as salted hashes, ensuring that a database breach does not compromise active user sessions.
- **JWT Integrity**: Signed with HMAC SHA-256 using a 32+ character secret key, with strict Issuer and Audience validation.
