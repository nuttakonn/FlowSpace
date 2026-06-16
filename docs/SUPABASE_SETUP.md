# FlowSpace Supabase Setup Guide

This document provides instructions for configuring and provisioning a **Supabase PostgreSQL 17** instance for the FlowSpace production environment.

## 1. Supabase Project Configuration

1. **Create Project**: Sign in to Supabase and create a new project.
2. **Database Version**: Ensure you are using PostgreSQL 17.
3. **SSL Enforcement**: 
   - Go to `Settings` -> `Database`.
   - Ensure `SSL` is enabled.
   - Note down the `Connection string` (specifically the URI and the ADO.NET format).
4. **Connection Pooling**:
   - Use the **Transaction** mode connection string (usually port `6543`) for the application backend to handle high-concurrency SignalR connections efficiently.

---

## 2. Remote Migrations

To structure the production database, use the provided migration script. This script uses Entity Framework Core to apply all schema changes.

### Execution
Run the following command from the repository root:
```bash
./scripts/database/migration.sh "Host=your-host.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
```

*Note: Ensure you have the `dotnet-ef` global tool installed.*

---

## 3. Database Seeding

Populate the database with initial system templates (Software Architecture, Mindmaps, etc.) using the seed script.

### Execution
The seed script requires `psql` to be installed on your machine.
```bash
export DATABASE_URL="postgres://postgres:your-password@your-host.supabase.co:6543/postgres"
./scripts/database/seed.sh
```

---

## 4. Connection String for Cloud Deployment (Koyeb/Render)

When configuring your Backend API in the cloud, use the following environment variable format to ensure compatibility with Supabase's SSL requirements:

| Variable | Recommended Value |
| :--- | :--- |
| `ConnectionStrings__DefaultConnection` | `Host=...;Port=6543;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true;` |

---

## 5. Maintenance & Backups

Supabase provides automated daily backups. However, for critical production data, it is recommended to:
1. Enable **Point-in-Time Recovery (PITR)** in the Supabase dashboard.
2. Regularly test the `restore.sh` script (found in `infrastructure/scripts/`) by restoring a production backup into a local staging environment.
