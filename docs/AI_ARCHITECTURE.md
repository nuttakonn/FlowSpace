# AI Diagram Generator Architecture

## Overview
FlowSpace integrates the **Gemini API** to provide an AI-native diagram generation experience. Users can describe complex systems, processes, or ideas in natural language, and the AI will generate a structured, editable visual representation.

---

## 1. Core Integration: Gemini API

- **Model**: Gemini 1.5 Flash (for speed) or Pro (for complex layouts).
- **Format**: Structured Output (JSON Mode) is used to ensure the AI returns a parseable diagram schema.
- **System Instruction**: A specialized prompt defining the "Expert Architect" role and the strict output format required by the FlowSpace Canvas.

---

## 2. AI Input Schema (Request)

The input to the AI includes the user's prompt and contextual data about the target board.

```json
{
  "prompt": "Create a microservices architecture for a fintech app with payment and wallet services.",
  "boardType": "Flowchart", 
  "context": {
    "existingNodes": [], 
    "theme": "modern"
  },
  "constraints": {
    "maxNodes": 20,
    "language": "English"
  }
}
```

---

## 3. AI Output Schema (Response)

The AI is instructed to return a flattened graph representation that maps easily to the FlowSpace `nodes` and `edges` tables.

```json
{
  "nodes": [
    {
      "id": "node_1",
      "type": "Rectangle",
      "label": "API Gateway",
      "description": "Entry point for all requests",
      "metadata": { "color": "#3b82f6" }
    },
    {
      "id": "node_2",
      "type": "Rectangle",
      "label": "Payment Service",
      "description": "Handles transactions",
      "metadata": { "color": "#10b981" }
    }
  ],
  "edges": [
    {
      "id": "edge_1",
      "source": "node_1",
      "target": "node_2",
      "label": "gRPC"
    }
  ],
  "layoutHint": "vertical" 
}
```

---

## 4. Mapping Rules

### Node Mapping
| AI Generated Category | FlowSpace Node Type | Metadata Mapping |
| :--- | :--- | :--- |
| Process/Action | `Rectangle` | Label in center. |
| Decision/Branch | `Diamond` | Conditional text label. |
| Start/End | `Circle` | Border thickness/style. |
| Entity/Service | `Rectangle` | Rounded corners = true. |
| Idea/Topic | `StickyNote` | Background color set to yellow-ish. |

### Edge Mapping
- **Source/Target**: IDs provided by the AI are mapped to fresh UUIDv7 identifiers in the database.
- **Routing**: In `Flowchart` mode, edges are automatically set to `smoothstep` (orthogonal). In `Mindmap` mode, they are set to `simplebezier`.

---

## 5. Diagram Support Logic

1. **Flowchart**: AI prioritizes `Rectangle` and `Diamond` nodes. Uses a top-down or left-right flow logic.
2. **System Architecture Diagram**: AI uses `Rectangle` nodes with rich metadata (e.g., icons for Database, Cache, API). Focuses on layering (Frontend, Backend, Data).
3. **User Journey**: AI generates a horizontal sequence of steps. Each node type is `Rectangle` but styled as "Steps".
4. **Mindmap**: AI generates a tree structure. Each child node contains a reference to its parent.

---

## 6. Layout Engine

Since the AI provides structural logic but not precise coordinates (X, Y), FlowSpace uses a server-side layout worker:
- **Engine**: `elkjs` or `dagre`.
- **Workflow**:
    1. Receive AI JSON.
    2. Pass nodes/edges to the layout engine.
    3. Generate optimal X/Y coordinates to prevent overlaps.
    4. Return final positioned elements to the client.

---

## 7. Validation Rules

- **ID Integrity**: Every edge `source` and `target` must match an existing node `id` in the same response.
- **Node Type Whitelist**: AI is forbidden from generating types not supported by the current `BoardType`.
- **Label Limits**: Labels are truncated at 100 characters to prevent UI breaking.

---

## 8. Error Recovery Strategy

| Failure Scenario | Recovery Action |
| :--- | :--- |
| **Invalid JSON** | Automatic retry with a "Fix JSON" correction prompt (max 1 retry). |
| **Orphaned Edges** | The parser automatically strips edges whose source or target is missing. |
| **Too Complex** | If the AI generates > 50 nodes, the system simplifies the graph before sending to the layout engine. |
| **API Timeout** | User is notified via toast; system offers an "Extend Wait" or "Cancel" option. |

---

## 9. Integration Flow (Sequence)

1. **Frontend**: User enters prompt and clicks "Generate".
2. **API**: `POST /api/v1/ai/generate` is called.
3. **Gemini**: API calls Gemini with system instructions.
4. **Layout**: AI response is fed into the layout engine for coordinate generation.
5. **Database**: Positioned nodes/edges are batch-inserted into PostgreSQL.
6. **Real-time**: SignalR broadcasts a "DiagramGenerated" event to the client.
7. **Frontend**: The canvas automatically zooms to fit the new elements.
