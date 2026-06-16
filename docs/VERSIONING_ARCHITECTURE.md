# Canvas Versioning Architecture

## Overview
FlowSpace provides a robust versioning system to allow users to save progress, track changes over time, and revert to previous states of their visual boards. This system is designed to handle high-volume data (100k nodes) efficiently by leveraging periodic snapshots and incremental change tracking.

---

## 1. Entities

### BoardVersion
Represents a named or automatic milestone in the board's history.

| Property | Type | Description |
| :--- | :--- | :--- |
| **Id** | UUID | Unique identifier (UUIDv7). |
| **BoardId** | UUID | Reference to the parent Board. |
| **Name** | String | Optional user-provided name (e.g., "v1.0", "Final Draft"). |
| **Description** | String | Optional notes about the version. |
| **CreatedAt** | DateTime | When the version was created. |
| **CreatedBy** | UUID | Reference to the User who created the version. |
| **SnapshotId** | UUID | Reference to the associated CanvasSnapshot. |
| **Type** | Enum | `Manual` (User-saved) or `Automatic` (System-generated). |

### CanvasSnapshot
A full point-in-time state of the board, stored in a highly compressed format.

| Property | Type | Description |
| :--- | :--- | :--- |
| **Id** | UUID | Unique identifier. |
| **BoardId** | UUID | Reference to the Board. |
| **NodesData** | JSONB | Array of all nodes and their state at that moment. |
| **EdgesData** | JSONB | Array of all edges at that moment. |
| **YjsState** | ByteA | Binary blob of the Yjs document state for full synchronization restoration. |
| **CreatedAt** | DateTime | Timestamp. |

---

## 2. Storage Strategy

### Snapshot vs. Differential
To support boards with up to 100,000 nodes, we cannot store a full JSON snapshot for every single minor change.

1. **Full Snapshots**: Created when a user manually "Saves a Version" or every 100 significant mutations. 
   - Stored in the `canvas_snapshots` table.
   - `NodesData` and `EdgesData` are stored as JSONB to allow for quick previewing and diffing.
2. **Incremental Changes**: For real-time "Restore" and "Undo/Redo" beyond the local session, the Yjs binary updates are the source of truth. However, `BoardVersion` strictly points to a `CanvasSnapshot`.

---

## 3. Versioning Workflows

### Save Snapshot (Creation)
1. **Trigger**: User clicks "Save Version" or an automated scheduled task runs.
2. **Collection**: The backend retrieves the current authoritative state from the `nodes` and `edges` tables for the board.
3. **Storage**:
   - A new `CanvasSnapshot` is inserted with the current state.
   - A new `BoardVersion` record is created, linking the User, the Board, and the Snapshot.
4. **Pruning**: To prevent database bloat, `Automatic` versions are pruned (e.g., keep 10 most recent, plus 1 per day for a week). `Manual` versions are kept indefinitely.

### Restore Version (Restoration)
1. **Selection**: User selects a version from the "History" panel.
2. **Transaction**:
   - The backend starts a database transaction.
   - **Soft-delete** all current `nodes` and `edges` for the board.
   - **Batch Insert** the nodes and edges from the `CanvasSnapshot.NodesData` and `EdgesData`.
   - Update the `Board.UpdatedAt` timestamp.
3. **Sync Refresh**:
   - The backend broadcasts a `VersionRestored` signal via SignalR.
   - All connected clients are notified to clear their local Yjs document and reload from the new authoritative state.

---

## 4. Change Tracking & Diffing

### Visual Diffing
Users can compare two versions before restoring.
1. The frontend fetches two snapshots.
2. It calculates the delta:
   - **Added**: Present in v2 but not v1.
   - **Removed**: Present in v1 but not v2.
   - **Modified**: Present in both but coordinates or metadata differ.
3. React Flow highlights these changes (e.g., Green for Added, Red for Removed).

### Performance Considerations
- **Compression**: For very large boards, JSONB data should be compressed at the application level (e.g., Gzip) before being stored in a `ByteA` column instead of raw `JSONB`, depending on the frequency of access vs. storage cost.
- **Async Processing**: Snapshot creation for 100k nodes should be handled by a background worker to avoid blocking the API request.
