# Real-time Collaboration Architecture

## Overview
FlowSpace enables seamless, low-latency real-time collaboration using **Yjs** (CRDT engine) and **SignalR** (transport layer). This architecture ensures that multiple users can edit the same board simultaneously with automatic conflict resolution, real-time presence tracking, and robust offline recovery.

---

## 1. Core Technology Stack
- **Yjs**: A high-performance Conflict-free Replicated Data Type (CRDT) library that manages the shared state.
- **SignalR**: ASP.NET Core's real-time framework used as the transport provider for Yjs binary updates and awareness data.
- **MessagePack**: Used for binary serialization of SignalR messages to minimize payload size.

---

## 2. Yjs Document Structure
Each Board maps to a unique `Y.Doc`. The document is organized into specific shared types:

### Shared Maps
- **`nodes` (Y.Map)**: 
    - Key: `NodeId` (UUID string).
    - Value: A JSON object containing the node's position, type, and metadata.
- **`edges` (Y.Map)**:
    - Key: `EdgeId` (UUID string).
    - Value: A JSON object containing source/target IDs and metadata.

### Shared Text (Optional)
- **`node_content_{nodeId}` (Y.Text)**: Used for granular collaborative text editing inside specific nodes (e.g., sticky notes or documentation blocks).

---

## 3. SignalR Collaboration Hub
The `CollaborationHub` is the central authority for routing updates.

### Hub Methods (Client to Server)
- `JoinBoard(boardId)`: Authenticates the user and joins a SignalR group for the specific board.
- `SendUpdate(boardId, update)`: Sends a binary Yjs update (diff) to the server.
- `SendAwareness(boardId, state)`: Sends ephemeral presence data (cursor position, selection).

### Hub Events (Server to Client)
- `OnUpdate(update)`: Broadcasts a Yjs binary update to all other members in the board group.
- `OnAwareness(userId, state)`: Broadcasts user presence changes.
- `OnUserJoined(userId, displayName)`: Notifies the group of a new collaborator.
- `OnUserLeft(userId)`: Notifies the group when a user disconnects.

---

## 4. Client Synchronization Flow

1. **Authentication**: Client connects to `CollaborationHub` with a JWT.
2. **Initialization**:
    - Client requests the initial board state skeleton via REST API.
    - Client invokes `JoinBoard(boardId)`.
3. **State Sync**:
    - The server sends the current authoritative Yjs state vector (cached in Redis or loaded from DB).
    - Client initializes local `Y.Doc` and applies the remote state.
4. **Local Mutations**:
    - User moves a node -> React Flow updates local view -> `onNodesChange` triggers Yjs mutation.
    - Yjs generates a binary diff -> Client sends `SendUpdate` via SignalR.
5. **Remote Mutations**:
    - `OnUpdate` received -> Client applies diff to local `Y.Doc`.
    - `Y.Doc` observer fires -> Zustand store updates `visibleNodes` -> React Flow re-renders.

---

## 5. Presence & Awareness
User presence is handled via the **Yjs Awareness** protocol, proxied through SignalR.

### Awareness State
```json
{
  "user": {
    "id": "uuid",
    "name": "John Doe",
    "color": "#FF5733"
  },
  "cursor": { "x": 120, "y": 450 },
  "selection": ["node-id-1", "node-id-2"],
  "lastActive": "2026-06-12T10:00:00Z"
}
```
- **Lifecycle**: Awareness data is purely ephemeral and stored in-memory (Redis) on the server. It is never persisted to the PostgreSQL database.
- **Rendering**: Remote cursors are rendered as a custom React Flow overlay.

---

## 6. Conflict Resolution & Consistency
- **CRDT Integrity**: Yjs ensures that all clients eventually converge to the same state, regardless of the order in which updates are received.
- **Server Persistence**: 
    - The backend acts as a "Headless Client". It maintains its own `Y.Doc` in memory for active boards.
    - Every N seconds or after significant activity, the server-side `Y.Doc` is serialized and persisted to the `nodes` and `edges` tables in PostgreSQL.
- **Validation**: The server validates incoming updates to ensure users have `Editor` permissions for the specific board before broadcasting.

---

## 7. Offline Support & Reconnect
- **Offline Editing**: Since Yjs is local-first, users can continue editing while offline. All mutations are stored in the local `Y.Doc` history.
- **Resync**: Upon reconnecting to SignalR:
    1. Client and Server exchange state vectors.
    2. Missing updates from the offline period are calculated and synchronized in both directions.
    3. Conflict resolution is applied automatically by Yjs.
- **Connection Persistence**: The `apiClient`'s retry logic and SignalR's automatic reconnection ensure a smooth experience during transient network loss.
