# FlowSpace Beta Release Checklist

This document details the final end-to-end verification and sign-off procedures required to transition FlowSpace from development to **Beta Release**.

## 1. Core Platform Verification

### 1.1 Infrastructure & Deployment
- [ ] **Frontend**: Deploy to Vercel. Verify edge routing and HTTPS.
- [ ] **Backend**: Deploy to Render/Koyeb. Verify container startup and Port 8080 exposure.
- [ ] **Database**: Provision Supabase instance. Run `migration.sh` and `seed.sh`.
- [ ] **Cache**: Provision Upstash Redis. Verify TLS connection via SignalR.
- [ ] **Health**: `GET /health` returns `Healthy` status for both API and Database.

### 1.2 Authentication Flow
- [ ] **Registration**: Create a new user account. Verify database entry.
- [ ] **Onboarding**: Complete the 3-step onboarding wizard after signup.
- [ ] **Login/Logout**: Verify JWT issuance and refresh token hashing/invalidation.
- [ ] **Security**: Verify `AuthLimit` rate limiter blocks 10+ attempts/min.

---

## 2. Feature Smoke Test (Manual)

### 2.1 Workspace & Canvas
- [ ] **Workspace**: Create a new workspace and verify sidebar update.
- [ ] **Board**: Create a "Flowchart" board. Drag 3 nodes and connect them.
- [ ] **Persistence**: Reload the page. Verify nodes remain in exact coordinates.

### 2.2 Multiplayer & Realtime
- [ ] **Awareness**: Open the board in Browser A and Browser B. Verify cursor names and colors appear.
- [ ] **Live Edits**: Move a node in Browser A. Verify sub-second movement in Browser B.
- [ ] **Selection**: Select a node in Browser A. Verify blue highlight in Browser B.

### 2.3 Whiteboard (tldraw)
- [ ] **Drawing**: Use the pencil tool to draw a complex shape.
- [ ] **Persistence**: Close tab and reopen. Verify the CRDT state vector rehydrates perfectly.

### 2.4 AI Integration (Gemini 1.5)
- [ ] **Prompt**: Enter "Create a user auth flow".
- [ ] **Preview**: Verify "Diff Preview" mode renders dashed nodes.
- [ ] **Apply**: Accept the AI suggestion and verify conversion to persistent database nodes.

---

## 3. Sharing & Export

### 3.1 Public Sharing
- [ ] **Link Generation**: Create a "Viewer" link.
- [ ] **Anonymous Check**: Open in Incognito. Verify board is visible but tools are disabled.
- [ ] **CORS**: Verify no console errors during cross-domain WebSocket handshake.

### 3.2 Professional Export
- [ ] **Native**: Download `.flowspace` file. Verify valid JSON schema.
- [ ] **draw.io**: Download `.drawio` file. Import into diagrams.net.
- [ ] **Visual**: Trigger PDF export. Verify Playwright background worker renders high-fidelity image.

---

## 4. Final Sign-off

- [x] **Security Audit**: All critical IDOR and cryptographic issues resolved.
- [x] **Monitoring**: OpenTelemetry and WebVitals integrated.
- [x] **CI/CD**: Auto-deployment configured for Vercel and Render.

---

**Release Status**: [ ] **READY** | [ ] **BLOCKED**

**Sign-off Date**: _________________________  
**Beta Version**: 0.1.0-beta
