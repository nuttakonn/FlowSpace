# FlowSpace Upstash Redis Setup Guide

This document provides instructions for configuring **Upstash Serverless Redis** for the FlowSpace production environment.

## 1. Upstash Configuration

1. **Create Database**: Sign in to [Upstash](https://console.upstash.com/) and create a new Global or Regional Redis database.
2. **TLS/SSL**: Ensure TLS is enabled (Upstash provides this by default).
3. **Connection String**: 
   - Copy the **Redis URL** (Format: `rediss://:password@host:port`).
   - Upstash uses the `rediss://` protocol for secure TLS connections.

---

## 2. Application Integration

FlowSpace uses Redis for three critical production features:
1. **Distributed Cache**: Used by `IPermissionService` to cache RBAC roles, reducing database load.
2. **SignalR Backplane**: Enables real-time collaboration scaling by synchronizing messages across multiple API instances.
3. **Rate Limiting**: (Future) Prevents abuse across multiple instances.

### Environment Variable Mapping

The backend is configured to prioritize the `REDIS_URL` environment variable.

| Variable | Recommended Value |
| :--- | :--- |
| `REDIS_URL` | `rediss://:your-password@your-host.upstash.io:6379` |

---

## 3. Technical Implementation Details

### Connection String Normalization
The backend includes a `RedisConfiguration` helper that automatically transforms the `rediss://` URL into a format compatible with `StackExchange.Redis`:
- **SSL**: Automatically sets `ssl=true`.
- **Port**: Correctly parses the secure port (default `6379` or `6380`).
- **Resilience**: Sets `abortConnect=false` to ensure the application starts even if Redis is temporarily unreachable.

### SignalR Configuration
SignalR is automatically wired to use the Redis backplane if `REDIS_URL` is detected. This is essential for features like **Multiplayer Cursors** and **Live Canvas Updates** to work correctly in a cloud-scaled environment.

---

## 4. Verification

To verify the connection:
1. Deploy the backend with the `REDIS_URL` environment variable.
2. Check the logs for any `StackExchange.Redis.RedisConnectionException`.
3. Open a board in two separate browsers and verify that cursors sync instantly. If cursors sync, the SignalR Redis backplane is operational.
