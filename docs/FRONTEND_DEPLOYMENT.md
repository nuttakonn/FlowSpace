# FlowSpace Frontend Production Deployment Guide (Vercel)

This document details the configuration and verification steps for deploying the FlowSpace Next.js frontend to **Vercel**.

## 1. Vercel Configuration

Vercel is the recommended hosting platform for the FlowSpace frontend due to its edge-optimized routing and deep integration with Next.js.

### GitHub Integration
1. Go to the [Vercel Dashboard](https://vercel.com/dashboard).
2. Click **"New Project"**.
3. Import your GitHub repository.
4. Set the **Root Directory** to `apps/web`.

### Build Settings
- **Framework Preset**: Next.js (Automatic).
- **Build Command**: `npm run build`.
- **Install Command**: `npm install` or `npm ci`.

---

## 2. Environment Variables

Add these variables in the **Settings -> Environment Variables** section of your Vercel project:

| Key | Description | Example |
| :--- | :--- | :--- |
| `NEXT_PUBLIC_API_URL` | The REST API endpoint. | `https://api.flowspace.app/api/v1` |
| `NEXT_PUBLIC_HUB_URL` | The SignalR real-time hub URL. | `https://api.flowspace.app/hubs/collaboration` |

*Note: Ensure the backend's `AllowedOrigins` includes your Vercel production URL (e.g. `https://flowspace.vercel.app`).*

---

## 3. Post-Deployment Verification (Smoke Test)

Once the deployment is finished, perform the following verification steps to ensure the frontend is communicating correctly with the cloud backend:

### 3.1 Authentication Flow
1. **Registration**: 
   - Navigate to `/register`.
   - Create a new account.
   - Verify that the API call returns `200 OK` and you are redirected to `/onboarding`.
2. **Login**: 
   - Log out and go to `/login`.
   - Sign in with your new credentials.
   - Verify that `accessToken` and `refreshToken` are stored in the application state/storage.
3. **Refresh Token**: 
   - Wait for the token to expire or manually trigger a request that returns `401`.
   - Verify in the Network tab that the frontend automatically calls `/auth/refresh` and successfully retries the original request with the new token.

### 3.2 API Communication
- Navigate to your **Workspaces**.
- Create a new workspace.
- Verify that the list updates immediately, indicating successful authenticated communication with the database via the API.

### 3.3 Real-time Check
- Open a **Board**.
- Open the same board in another browser/incognito tab.
- Verify that cursors and edits sync across both instances (confirms SignalR + Redis backplane connectivity).

---

## 4. Configuration File (`vercel.json`)

A `vercel.json` file is located in `apps/web/` to explicitly define build commands and the Next.js framework context. 
