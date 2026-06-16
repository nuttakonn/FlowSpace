#!/bin/bash

# ==============================================================================
# FlowSpace Remote Database Migration Script
# Uses EF Core Tools to apply migrations to a remote PostgreSQL instance.
# ==============================================================================

set -e

# Default to environment variable if not provided as argument
CONNECTION_STRING=${1:-$CONNECTION_STRINGS__DEFAULTCONNECTION}

if [ -z "$CONNECTION_STRING" ]; then
    echo "❌ Error: Connection string not provided."
    echo "Usage: $0 \"Host=...;Database=...;Username=...;Password=...\""
    exit 1
fi

echo "🚀 Starting database migrations against remote instance..."

# Ensure we are in the API root for the dotnet command
cd "$(dirname "$0")/../../apps/api"

# Apply migrations
# We use --project and --startup-project to ensure the correct context
# SSL Mode=Require is highly recommended for Supabase
export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"

dotnet ef database update \
    --project src/FlowSpace.Infrastructure \
    --startup-project src/FlowSpace.Api \
    --context ApplicationDbContext

if [ $? -eq 0 ]; then
    echo "✅ Migrations applied successfully."
else
    echo "❌ Migration failed!"
    exit 1
fi
