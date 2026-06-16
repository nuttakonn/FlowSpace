# FlowSpace Backup and Disaster Recovery Strategy

This document outlines the procedures for backing up the FlowSpace PostgreSQL database and executing disaster recovery operations to ensure business continuity and data integrity.

## 1. Overview

FlowSpace relies on a PostgreSQL 17 database running inside a Docker container. The backup strategy utilizes `pg_dump` to create compressed, custom-format (`-F c`) logical backups. This format allows for targeted restorations and minimizes storage requirements. 

Backup scripts are provided in the `infrastructure/scripts/` directory.

---

## 2. Automated Scheduled Backups

To ensure regular, automated backups without manual intervention, you must configure a `cron` job on the production host machine.

### Setup Instructions

1. **Create the Backup Directory:**
   ```bash
   sudo mkdir -p /opt/flowspace/backups
   sudo chown -R ubuntu:ubuntu /opt/flowspace/backups # Assuming 'ubuntu' is your deployment user
   ```

2. **Configure Cron Job:**
   Open the crontab editor:
   ```bash
   crontab -e
   ```

   Add the following line to schedule a backup every day at 02:00 AM (server time):
   ```cron
   # FlowSpace Daily Database Backup
   0 2 * * * /path/to/flowspace/infrastructure/scripts/backup.sh >> /var/log/flowspace_backup.log 2>&1
   ```
   *Replace `/path/to/flowspace` with your actual deployment path (e.g., `/var/www/flowspace`).*

### Configuration Options
The `backup.sh` script supports environment variable overrides:
- `BACKUP_DIR`: Defaults to `/opt/flowspace/backups`.
- `RETENTION_DAYS`: Number of days to keep backups before deleting them (Default: 7).
- `POSTGRES_CONTAINER`: Docker container name (Default: `flowspace-postgres`).

---

## 3. Offsite Backup Strategy (Highly Recommended)

Storing backups on the same server as the database is not a complete disaster recovery plan. If the server is destroyed, the backups are lost.

You **MUST** implement an offsite sync mechanism. 

### AWS S3 Sync Example
Configure the AWS CLI on your host and add a sync command to your cron job, running shortly after the backup completes (e.g., at 02:15 AM):

```cron
15 2 * * * aws s3 sync /opt/flowspace/backups s3://your-company-flowspace-backups/database/ --delete
```

---

## 4. Disaster Recovery Procedure (Restoration)

If the database is corrupted, data is accidentally deleted, or you are migrating to a new server, use the `restore.sh` script to recover the database from a `.sql.gz` dump.

### ⚠️ Important Warnings
- **Restoration is destructive.** The script drops the existing database and replaces it entirely with the contents of the backup file.
- It is highly recommended to stop the API and Web containers before initiating a restore to prevent partial writes during the process.

### Step-by-Step Restoration

1. **Stop Application Traffic (Optional but Recommended):**
   Navigate to your deployment directory and stop the frontend and backend services:
   ```bash
   docker-compose -f docker-compose.production.yml stop api web
   ```
   *Leave `postgres` and `redis` running.*

2. **Locate the Backup File:**
   Identify the backup file you wish to restore from your backup directory or download it from your offsite storage (S3).
   ```bash
   ls -la /opt/flowspace/backups/
   ```

3. **Execute the Restore Script:**
   Run `restore.sh` and pass the absolute path to the backup file as the first argument.
   ```bash
   ./infrastructure/scripts/restore.sh /opt/flowspace/backups/flowspace_prod_backup_20260612_020000.sql.gz
   ```

4. **Confirm Execution:**
   The script will prompt you to confirm the destructive operation. Type `yes` and press Enter.

5. **Restart Application Traffic:**
   Once the script outputs `✅ Restore completed successfully.`, bring the application services back online:
   ```bash
   docker-compose -f docker-compose.production.yml start api web
   ```

### Troubleshooting Restoration
If the restore fails, check the Docker logs for the Postgres container:
```bash
docker logs flowspace-postgres --tail 100
```
Common issues include incorrect database names or the postgres user lacking `CREATEDB` privileges (which the `flowspace_admin` user should have by default).
