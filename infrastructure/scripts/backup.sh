#!/bin/bash

# ==============================================================================
# FlowSpace Database Backup Script
# This script creates a compressed dump of the PostgreSQL database running inside
# the Docker container. It retains a configurable number of historical backups.
# ==============================================================================

# Exit immediately if a command exits with a non-zero status
set -e

# --- Configuration ---
# You can override these via environment variables before running the script
CONTAINER_NAME=${POSTGRES_CONTAINER:-"flowspace-postgres"}
DB_USER=${POSTGRES_USER:-"flowspace_admin"}
DB_NAME=${POSTGRES_DB:-"flowspace_prod"}
BACKUP_DIR=${BACKUP_DIR:-"/opt/flowspace/backups"}
RETENTION_DAYS=${RETENTION_DAYS:-7}

# Timestamp for the backup file
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_backup_${TIMESTAMP}.sql.gz"

echo "Starting backup of database '${DB_NAME}'..."
echo "Destination: ${BACKUP_FILE}"

# Ensure backup directory exists
mkdir -p "${BACKUP_DIR}"

# --- Backup Execution ---
# We use pg_dump inside the container and pipe the output to gzip on the host
# This minimizes disk usage inside the container itself.
docker exec -t "${CONTAINER_NAME}" pg_dump -U "${DB_USER}" -d "${DB_NAME}" -F c | gzip > "${BACKUP_FILE}"

if [ $? -eq 0 ]; then
    echo "✅ Backup completed successfully."
    echo "File size: $(du -sh "${BACKUP_FILE}" | cut -f1)"
else
    echo "❌ Backup failed!"
    exit 1
fi

# --- Retention Policy ---
echo "Cleaning up backups older than ${RETENTION_DAYS} days..."
find "${BACKUP_DIR}" -name "${DB_NAME}_backup_*.sql.gz" -type f -mtime +${RETENTION_DAYS} -exec rm -f {} \;
echo "Cleanup complete."

echo "Backup process finished."
