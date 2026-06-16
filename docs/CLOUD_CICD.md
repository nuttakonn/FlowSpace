# FlowSpace Cloud CI/CD Guide

This document outlines the automated Continuous Integration and Continuous Deployment pipelines for FlowSpace, optimized for cloud-native platforms (**Vercel**, **Render**, and **Koyeb**).

## 1. Pipeline Architecture

FlowSpace utilizes **GitHub Actions** to orchestrate the lifecycle of the application across two primary stages:

### 1.1 Continuous Integration (`ci.yml`)
Runs on every **Pull Request** targeting the `main` branch.
- **Backend API**: Restores, builds, and executes all `xUnit` integration and unit tests.
- **Web Frontend**: Installs dependencies and performs a production Next.js build to catch build-time errors.
- **Goal**: Prevent code regressions and structural errors from entering the main codebase.

### 1.2 Continuous Deployment (`cd.yml`)
Runs automatically on every **Push** to the `main` branch.
- **Backend API**: Triggers a **Deploy Hook** (webhook) for Render or Koyeb, instructing the platform to pull the latest code and rebuild the Docker container.
- **Web Frontend**: Uses the **Vercel CLI Action** to build and deploy the `apps/web` directory directly to Vercel production.
- **Goal**: Zero-touch delivery of new features to the cloud.

---

## 2. GitHub Secrets Configuration

To enable the deployment pipeline, you must configure the following **Repository Secrets** in your GitHub repository (`Settings` -> `Secrets and variables` -> `Actions`):

### 2.1 Backend Secrets (Render / Koyeb)
| Secret Name | Description |
| :--- | :--- |
| `BACKEND_DEPLOY_HOOK` | The full Deploy Hook URL provided by your backend host (e.g., `https://api.render.com/deploy/srv-...`). |

### 2.2 Frontend Secrets (Vercel)
| Secret Name | Description |
| :--- | :--- |
| `VERCEL_TOKEN` | Your Vercel Personal Access Token (found in Account Settings). |
| `VERCEL_ORG_ID` | Your Vercel Organization ID (found in Project Settings or CLI). |
| `VERCEL_PROJECT_ID` | Your Vercel Project ID (found in Project Settings). |

---

## 3. Best Practices for Cloud CI/CD

### 3.1 Pre-Deployment Migrations
The backend is configured to **auto-migrate** on startup in cloud environments. This means the CD pipeline handles both code updates and schema updates in a single step. Ensure your `ApplyMigrationsOnStartup` is set to `true` in your backend environment variables.

### 3.2 Environment Variable Management
**Important**: GitHub Actions handles the **triggering** of the build, but the **secrets** (DB strings, API keys) must be configured directly in the dashboards of Vercel and Render/Koyeb.
- Vercel: **Project Settings -> Environment Variables**.
- Render/Koyeb: **Service Settings -> Environment Variables**.

### 3.3 Monitoring Deployments
- Check the **GitHub Actions** tab to see if the pipeline triggered successfully.
- Monitor the **Render/Koyeb** logs to see the Docker build progress.
- Verify the **Vercel** dashboard for successful edge deployment.
