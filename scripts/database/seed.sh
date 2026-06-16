#!/bin/bash

# ==============================================================================
# FlowSpace Database Seeding Script
# Uses psql to apply initial system data to the PostgreSQL instance.
# ==============================================================================

set -e

# Default to environment variable if not provided as argument
# Format: postgres://user:password@host:port/dbname
DB_URL=${1:-$DATABASE_URL}

if [ -z "$DB_URL" ]; then
    echo "❌ Error: Database URL (postgres://...) not provided."
    echo "Usage: $0 \"postgres://user:password@host:port/dbname\""
    exit 1
fi

echo "🌱 Starting database seeding..."

# Use psql to run the seed script
# --set ON_ERROR_STOP=on ensures the script stops if a command fails
psql "$DB_URL" -f "$(dirname "$0")/seed.sql" --set ON_ERROR_STOP=on

if [ $? -eq 0 ]; then
    echo "✅ Seeding completed successfully."
else
    echo "❌ Seeding failed!"
    exit 1
fi
