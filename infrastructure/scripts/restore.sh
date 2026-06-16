#!/bin/bash

# ==============================================================================
# FlowSpace Database Restore Script
# This script restores a PostgreSQL database from a compressed dump file created
# by the backup.sh script into the running Docker container.
# WARNING: THIS WILL OVERWRITE EXISTING DATA.
# ==============================================================================

# Exit immediately if a command exits with a non-zero status
set -e

# --- Configuration ---
CONTAINER_NAME=${POSTGRES_CONTAINER:-"flowspace-postgres"}
DB_USER=${POSTGRES_USER:-"flowspace_admin"}
DB_NAME=${POSTGRES_DB:-"flowspace_prod"}

# Ensure a backup file is provided as an argument
if [ -z "$1" ]; then
    echo "Usage: $0 <path_to_backup_file.sql.gz>"
    echo "Example: $0 /opt/flowspace/backups/flowspace_prod_backup_20260612_100000.sql.gz"
    exit 1
fi

BACKUP_FILE="$1"

# Verify the file exists
if [ ! -f "${BACKUP_FILE}" ]; then
    echo "❌ Error: Backup file '${BACKUP_FILE}' not found."
    exit 1
fi

# --- Confirmation ---
echo "⚠️  WARNING: You are about to restore the database '${DB_NAME}' from '${BACKUP_FILE}'."
echo "This operation will drop existing connections and OVERWRITE current data."
read -p "Are you sure you want to proceed? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo "Restore aborted by user."
    exit 0
fi

echo "Starting restoration process..."

# --- Restore Execution ---
# 1. Terminate existing connections
echo "Terminating existing database connections..."
docker exec -i "${CONTAINER_NAME}" psql -U "${DB_USER}" -d postgres -c "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '${DB_NAME}' AND pid <> pg_backend_pid();"

# 2. Drop and recreate the database to ensure a clean slate
echo "Recreating database '${DB_NAME}'..."
docker exec -i "${CONTAINER_NAME}" psql -U "${DB_USER}" -d postgres -c "DROP DATABASE IF EXISTS ${DB_NAME};"
docker exec -i "${CONTAINER_NAME}" psql -U "${DB_USER}" -d postgres -c "CREATE DATABASE ${DB_NAME};"

# 3. Restore the data
# We decompress the file on the host and pipe it directly to pg_restore inside the container
echo "Restoring data from ${BACKUP_FILE}..."
gunzip -c "${BACKUP_FILE}" | docker exec -i "${CONTAINER_NAME}" pg_restore -U "${DB_USER}" -d "${DB_NAME}" --no-owner --role="${DB_USER}"

if [ $? -eq 0 ]; then
    echo "✅ Restore completed successfully."
else
    echo "❌ Restore encountered errors. Please check the logs."
    exit 1
fi
