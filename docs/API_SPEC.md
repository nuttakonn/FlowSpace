# API Specification

## Overview
This document defines the REST API for FlowSpace v1. All endpoints are relative to the base URL.

**Base URL**: `https://api.flowspace.app/api/v1`

## Authentication
All requests (except Login and Register) must include a Bearer Token in the `Authorization` header.

```http
Authorization: Bearer <jwt_token>
```

---

## Standard Error Response
FlowSpace follows the [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) standard.

```json
{
  "type": "https://flowspace.app/probs/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The Name field is required.",
  "instance": "/api/v1/workspaces",
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

---

## 1. Authentication

### POST `/auth/register`
Create a new user account.
- **Request**: `{ "email": "user@example.com", "password": "securePassword123", "displayName": "John Doe", "inviteCode": "secret-code" }`
- **Response (200)**: `{ "id": "uuid", "email": "user@example.com", "displayName": "John Doe", "accessToken": "jwt...", "refreshToken": "uuid..." }`

### POST `/auth/login`
Authenticate and receive tokens.
- **Request**: `{ "email": "user@example.com", "password": "securePassword123" }`
- **Response (200)**: `{ "accessToken": "jwt...", "refreshToken": "uuid...", "expiresIn": 900 }`
- **Note**: Tokens are also set as HttpOnly cookies.

### POST `/auth/refresh`
Refresh an expired access token.
- **Request**: `{ "refreshToken": "uuid..." }`
- **Response (200)**: `{ "accessToken": "jwt...", "refreshToken": "uuid..." }`

---

## 2. Workspace

### GET `/workspaces`
List workspaces owned or shared with the user.
- **Auth**: Required
- **Response (200)**: `[{ "id": "uuid", "name": "Project Alpha", "ownerId": "uuid" }]`

### POST `/workspaces`
Create a new workspace.
- **Auth**: Required
- **Request**: `{ "name": "New Workspace" }`
- **Response (201)**: Created workspace object.

### GET `/workspaces/{id}`
Get details of a specific workspace, including board summary.
- **Auth**: Required (Owner/Member)
- **Response (200)**: Workspace details.

### DELETE `/workspaces/{id}`
Delete a workspace.
- **Auth**: Required (Owner)
- **Response (204)**: No Content.

---

## 3. Board

### GET `/workspaces/{workspaceId}/boards`
List boards within a workspace.
- **Auth**: Required (Member)
- **Response (200)**: List of boards.

### POST `/workspaces/{workspaceId}/boards`
Create a new board.
- **Auth**: Required (Owner/Editor)
- **Request**: `{ "name": "Sprint Map", "type": "Flowchart" }`
- **Response (201)**: Created board object.

### GET `/boards/{id}`
Get a board's full details (metadata only).
- **Auth**: Required (Viewer/Editor/Owner)
- **Response (200)**: Board object.

### PATCH `/boards/{id}`
Update board metadata (e.g., rename).
- **Auth**: Required (Editor/Owner)
- **Request**: `{ "name": "Updated Name" }`
- **Response (200)**: Updated board object.

---

## 4. Nodes & Edges (Canvas Data)

*Note: While real-time updates happen via SignalR/Yjs, REST endpoints are used for initial state retrieval and batch operations.*

### GET `/boards/{id}/elements`
Get all nodes and edges for a board.
- **Auth**: Required (Viewer)
- **Response (200)**: `{ "nodes": [...], "edges": [...] }`

### POST `/boards/{id}/nodes`
Create a node (usually for AI generation or batch imports).
- **Auth**: Required (Editor)
- **Request**: `{ "type": "Rectangle", "position": { "x": 10, "y": 20 }, "metadata": { "label": "Start" } }`
- **Response (201)**: Node object.

### PUT `/boards/{id}/nodes/batch`
Update multiple nodes simultaneously.
- **Auth**: Required (Editor)
- **Request**: `{ "nodes": [{ "id": "uuid", "position": { "x": 100, "y": 100 } }, ...] }`
- **Response (200)**: Success status.

### DELETE `/boards/{id}/nodes/{nodeId}`
Delete a node.
- **Auth**: Required (Editor)

---

## 5. Collaboration

### POST `/boards/{id}/sessions`
Register an active collaboration session.
- **Auth**: Required (Viewer)
- **Response (201)**: `{ "sessionId": "uuid", "signalRUrl": "wss://api.flowspace.app/collaboration?boardId=..." }`

### GET `/boards/{id}/active-users`
Get list of users currently editing the board.
- **Auth**: Required (Viewer)
- **Response (200)**: `[{ "userId": "uuid", "displayName": "John", "lastSeenAt": "timestamp" }]`

---

## 6. AI Generation

### POST `/ai/generate`
Generate diagram elements based on a prompt.
- **Auth**: Required (Editor)
- **Request**: `{ "boardId": "uuid", "prompt": "Create a microservices architecture diagram for an e-commerce app" }`
- **Response (202)**: Accepted (returns an `operationId` for polling or listens via SignalR).
