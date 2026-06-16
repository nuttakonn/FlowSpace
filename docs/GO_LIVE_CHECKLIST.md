# FlowSpace Go-Live Checklist & Production Smoke Test

This document serves as the final verification checklist before FlowSpace is released to production users. A comprehensive end-to-end smoke test has been designed to validate all critical paths across the stack.

## 1. Authentication & Security
- [ ] **Registration**: Successfully create a new user account.
- [ ] **Login**: Authenticate with the newly created credentials and receive a JWT.
- [ ] **Refresh Token**: Wait for token expiration (or manually trigger refresh) and verify a new JWT is securely issued.
- [ ] **Logout**: Verify the refresh token is invalidated in the database and the session is terminated.
- [ ] **Secrets Verification**: Ensure `.env` is securely loaded and no default/development secrets (e.g., test JWT keys) are present in the environment.

## 2. Workspace Management
- [ ] **Create Workspace**: Instantiate a new workspace.
- [ ] **Invite Member**: Send an invitation to another user email and verify they receive correct access (Viewer/Editor).
- [ ] **RBAC Enforcement**: Verify that a user cannot access or modify a workspace they were not invited to.

## 3. Board & Diagramming (React Flow)
- [ ] **Create Board**: Initialize a new "Flowchart" or "Mindmap" board.
- [ ] **Open Board**: Successfully load the canvas without console errors.
- [ ] **Edit Diagram**: Drag and drop nodes, connect edges, and edit node text.
- [ ] **Persistence**: Refresh the page and ensure all diagram elements are perfectly restored from the database.

## 4. Whiteboard (tldraw)
- [ ] **Draw**: Utilize the pen tool to draw freehand shapes.
- [ ] **Add Text**: Place sticky notes and text blocks.
- [ ] **Save & Reload**: Verify that the Yjs binary state is correctly persisted to PostgreSQL and rehydrates accurately upon page reload.

## 5. Real-time Collaboration (SignalR + Yjs)
- [ ] **Multiplayer Session**: Open the same board in two separate incognito windows or browsers.
- [ ] **Live Edits**: Draw on one window and observe sub-second updates on the other.
- [ ] **Awareness**: Verify that remote cursors (with names) and selection highlights are visible.
- [ ] **Disconnect Handling**: Close one tab and verify the cursor disappears; reopen and ensure state immediately synchronizes.

## 6. AI Generation (Gemini 1.5 Flash)
- [ ] **Generate Diagram**: Enter a natural language prompt (e.g., "Create a microservice architecture").
- [ ] **Preview Mode**: Verify the AI output renders in the "Diff Preview" state (dashed lines, distinct colors).
- [ ] **Apply**: Click "Accept" and verify the nodes are permanently merged into the canvas and saved to the database.
- [ ] **Refinement**: Test a refinement command like "Explain Diagram" and verify the AI appends an explanation sticky note.

## 7. Sharing & Access Control
- [ ] **Public Link Generation**: Create a share link with `Viewer` access.
- [ ] **Anonymous Access**: Open the link in an incognito window and verify the board renders correctly.
- [ ] **Permission Integrity**: Attempt to edit the board from the anonymous window and verify the UI/API blocks the mutation.
- [ ] **Revocation**: Revoke the share link and verify the anonymous window can no longer access the board.

## 8. Export & Interoperability
- [ ] **Native Export (.flowspace)**: Export the board and verify the downloaded JSON file is structurally sound.
- [ ] **draw.io Export**: Export as XML and verify it successfully imports into diagrams.net.
- [ ] **Visual Export (Playwright)**: Trigger a PDF or PNG export and verify the background worker successfully renders and returns the high-fidelity image.

## 9. Infrastructure & Deployment Health
- [ ] **Health Checks**: Access `/health` and verify it returns `Healthy` (confirming both the API and Database are communicating).
- [ ] **Log Ingestion**: Trigger an intentional error (e.g., invalid login) and verify the structured Serilog JSON appears in the log aggregator (CloudWatch, Datadog, etc.).
- [ ] **Database Backups**: Verify that `backup.sh` runs successfully and a `.sql.gz` artifact is generated in the target directory.
- [ ] **Reverse Proxy / SSL**: Verify HTTPS is enforced and WebSockets (wss://) upgrade successfully through Nginx/Traefik.

---

### Sign-off

**Executed By:** _________________________  
**Date:** _________________________  
**Status:** [ ] **READY FOR PRODUCTION** | [ ] **BLOCKED**
