# Canvas Architecture

## Overview
The FlowSpace Canvas is the core interactive component of the platform, built on top of **React Flow**. It must support multiple board types (Flowchart, Mindmap, Whiteboard, Wireframe) and scale to **100,000 nodes** per board.

This document outlines the architecture required to achieve real-time collaboration and extreme scalability.

---

## 1. Technology Stack
- **Rendering Engine**: React Flow (for structured nodes/edges and viewport management).
- **State Management**: Zustand (for local, non-collaborative state).
- **Collaboration Engine**: Yjs (for CRDT-based shared state).
- **Transport Layer**: SignalR (WebSockets).
- **Backend Persistence**: PostgreSQL with JSONB columns.

---

## 2. Frontend State Model & Selection Isolation

Handling 100k nodes requires a strict separation of local viewport/visual state and shared collaborative state.

### Yjs Shared State (The Source of Truth)
The CRDT document (`Y.Doc`) holds the authoritative state of the board.
- `yNodes`: A `Y.Map` where keys are Node IDs and values are Node objects (with visual properties like `selected` stripped).
- `yEdges`: A `Y.Map` where keys are Edge IDs and values are Edge objects.

### Local-Only Visual States (Selection Isolation)
Visual states that are transient and client-specific (e.g. selection focus outlines, resizing handles, and floating node action toolbars) are **strictly isolated** from the shared Yjs document:
1. **Property Stripping**: The `selected` state of nodes is stripped out before writing updates to the `yNodes`/`yEdges` maps (e.g., in `onNodesChange`, `updateNodeColor`, `saveNodePosition`).
2. **Observe Merging**: In the `localYNodes.observe` and `localYEdges.observe` listeners, updates from Yjs are merged into Zustand while preserving the current local `selected` properties to prevent multiple nodes from showing active outlines across users.
3. **Local Toolbars**: Selection toolbars are rendered floating directly on selected nodes but toggle visibility locally via this isolated state.

### Collapsible Shapes Toolbar
The left-hand shapes selector (`FloatingToolbar`) can be collapsed using the toggle trigger button (`ChevronLeft`/`ChevronRight`) at the canvas edge, allowing users to hide visual panel clutter when reviewing boards.

### React Flow State (The View Model)
React Flow expects an array of `Node` and `Edge` objects. We do **not** store all 100k nodes in React Flow's state simultaneously.

```typescript
// Zustand Store bridging Yjs and React Flow
interface CanvasState {
  nodes: Node[]; 
  edges: Edge[];
  past: Mutation[];
  future: Mutation[];
  mutationQueue: Mutation[];
  syncStatus: 'idle' | 'saving' | 'saved' | 'failed';
  
  // Actions
  onNodesChange: (changes: NodeChange[]) => void;
  onEdgesChange: (changes: EdgeChange[]) => void;
  addNode: (type: string, position: { x: number; y: number }) => void;
  selectAll: () => void;
}
```

---

## 3. Viewport Model & Culling (Scalability)

React Flow natively renders DOM elements for every node passed to it. 100k DOM elements will crash any browser. We must implement **Spatial Hashing** and **Viewport Culling**.

### Spatial Indexing (QuadTree / Spatial Grid)
- When nodes are added/moved in the `Y.Doc`, they are indexed into a local Spatial Grid (e.g., a 1000x1000 pixel grid system).
- The grid allows us to ask: *"Give me all node IDs that intersect with bounding box `[x1, y1, x2, y2]`"*.

### Culling Logic
1. React Flow's `onMove` event fires as the user pans or zooms.
2. We calculate the current Viewport Bounding Box (with a 20% buffer margin).
3. We query the Spatial Grid for intersecting Node IDs.
4. We extract those specific nodes from `yNodes` and push them to `visibleNodes` in Zustand.
5. React Flow only renders the `visibleNodes` (e.g., 200 nodes instead of 100,000).

---

## 4. Backend Persistence Model

The backend must efficiently store and retrieve canvas data.

### Node Schema
```sql
CREATE TABLE nodes (
    id UUID PRIMARY KEY,           -- UUIDv7 for clustered indexing
    board_id UUID NOT NULL,        -- FK to boards
    type VARCHAR(50) NOT NULL,     -- 'Rectangle', 'Text', 'Image', etc.
    x FLOAT8 NOT NULL,
    y FLOAT8 NOT NULL,
    width FLOAT8,
    height FLOAT8,
    metadata JSONB NOT NULL DEFAULT '{}', -- Flexible data payload
    version INT4 NOT NULL DEFAULT 1,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    
    -- Filtered Compound Index for fast retrieval
    CONSTRAINT fk_board FOREIGN KEY (board_id) REFERENCES boards(id) ON DELETE CASCADE
);

CREATE INDEX ix_nodes_board_id ON nodes (board_id) WHERE is_deleted = false;
CREATE INDEX ix_nodes_metadata ON nodes USING gin (metadata);
```

### Edge Schema
```sql
CREATE TABLE edges (
    id UUID PRIMARY KEY,
    board_id UUID NOT NULL,
    source_node_id UUID NOT NULL,
    target_node_id UUID NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}',
    is_deleted BOOLEAN NOT NULL DEFAULT false
);
CREATE INDEX ix_edges_board_id ON edges (board_id) WHERE is_deleted = false;
```

### JSONB Metadata Structure
The `metadata` JSONB column stores type-specific data, preventing schema explosion.

**Example: Text Node**
```json
{
  "text": "System Architecture",
  "fontSize": 24,
  "color": "#333333",
  "fontWeight": "bold"
}
```

**Example: Image Node**
```json
{
  "url": "https://storage.flowspace.app/images/arch.png",
  "altText": "Architecture diagram",
  "aspectRatio": 1.5
}
```

---

## 5. Lazy Loading Strategy

When a user opens a board with 100k nodes, we cannot send 100MB of JSON via REST immediately.

1. **Initial Load (REST)**: The client requests `/boards/{id}/elements?viewport=[x,y,w,h]`. The backend uses PostGIS or simple bounded box SQL queries to return *only* the nodes in the initial view.
2. **WebSocket Sync (Yjs/SignalR)**: The client connects to the SignalR Hub. The server sends a compressed Yjs state vector containing the structural skeleton of the board (IDs and basic positions).
3. **Progressive Fetching**: As the user pans the canvas, the client requests detailed metadata for nodes entering the viewport that it hasn't cached locally yet.

---

## 6. Board Type Contexts

While the underlying engine is the same, the UI behaves differently based on `Board.Type`:

- **Flowchart**: Snap-to-grid enabled by default. Edge routing is orthogonal. Node types restricted to geometric shapes.
- **Mindmap**: Auto-layout enabled (e.g., using `dagre` or `elkjs`). Adding a child node automatically positions it and creates an edge.
- **Whiteboard**: Free-form. Sticky notes, freehand drawing (via custom SVG nodes), and image support.
- **Wireframe**: Specialized nodes for UI components (buttons, inputs, browser windows).
