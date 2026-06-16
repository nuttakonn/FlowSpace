# Import/Export & Interoperability Architecture

## Overview
FlowSpace supports high-fidelity data mobility and interoperability with industry-standard tools like draw.io. This document defines the native `.flowspace` format, the mapping logic for external formats, and the scalable pipeline for processing large board datasets.

---

## 1. Native Format: `.flowspace`

The native format is a schema-versioned JSON file containing the complete state of a board.

### File Schema (`v1.0.0`)
```json
{
  "format": "flowspace",
  "version": "1.0.0",
  "metadata": {
    "id": "uuid",
    "name": "Project Architecture",
    "type": "Flowchart",
    "exportedAt": "2026-06-12T10:00:00Z",
    "generator": "FlowSpace-Web-0.4.2"
  },
  "canvas": {
    "viewport": { "x": 100, "y": 200, "zoom": 1.5 },
    "nodes": [
      {
        "id": "node-1",
        "type": "Rectangle",
        "position": { "x": 10, "y": 20 },
        "size": { "width": 100, "height": 50 },
        "data": { "label": "Start" },
        "style": { "backgroundColor": "#ffffff" }
      }
    ],
    "edges": [
      {
        "id": "edge-1",
        "source": "node-1",
        "target": "node-2",
        "type": "smoothstep",
        "data": {}
      }
    ]
  }
}
```

---

## 2. Supported Formats

| Format | Type | Support | Logic |
| :--- | :--- | :--- | :--- |
| **.flowspace** | Native (JSON) | Import/Export | Full state restoration. |
| **draw.io (XML)** | Interop (XML) | Import/Export | Structural & style mapping. |
| **SVG** | Vector (XML) | Export | High-quality vector graphics for docs. |
| **PDF** | Document | Export | Multi-page or single-canvas report. |
| **PNG** | Raster | Export | Fast previews and sharing. |

---

## 3. Mapping Rules (Interop with draw.io)

### Geometry Mapping
- **Coordinates**: FlowSpace uses top-left origin (0,0). draw.io uses a similar system but may require coordinate normalization if the canvas was panned.
- **Dimensions**: Strictly mapped (Width/Height).

### Node/Shape Mapping
| FlowSpace Type | draw.io Stencil (mxgraph) |
| :--- | :--- |
| Rectangle | `shape=rectangle` |
| Circle | `shape=ellipse` |
| Diamond | `shape=rhombus` |
| Text | `text;html=1;` |
| StickyNote | `shape=note` |

### Style Mapping
- **Colors**: Tailwind CSS standard classes (e.g., `bg-blue-500`) are resolved to HEX codes (`#3b82f6`) during export.
- **Lines**: Orthogonal routing in Flowchart mode maps to `edgeStyle=orthogonalEdgeStyle`.

---

## 4. Conversion Pipeline

The pipeline is split between client-side and server-side to handle various board sizes.

### Client-Side (Fast/Small Boards)
- **Library**: `reactflow-to-svg`, `jsPDF`.
- **Workflow**: 
    1. Direct DOM-to-Image serialization for PNG/SVG.
    2. JSON generation for `.flowspace`.
- **Threshold**: Boards < 1,000 nodes.

### Server-Side (Scalable/Large Boards)
For the 100,000 node success criterion, client-side serialization will hang the browser.
- **Service**: `FlowSpace.Infrastructure.ExportService`.
- **Workflow**:
    1. Client triggers Export API: `POST /boards/{id}/export?format=PDF`.
    2. Server retrieves snapshot from PostgreSQL (ignoring soft-deleted rows).
    3. **Headless Conversion**: A Worker uses a headless Chromium instance (Puppeteer/Playwright) to render the canvas at scale.
    4. **Streaming**: The resulting file is generated and uploaded to **MinIO**, providing a temporary download link to the user.

---

## 5. Version Compatibility

- **Forward Compatibility**: The `.flowspace` format includes a `version` field. Future versions of FlowSpace must include a `Migrator` utility to up-convert older files.
- **Backward Compatibility**: If a user imports a `v1.2.0` file into a `v1.0.0` application, the app should warn and only process known fields.

---

## 6. Implementation Strategy

1. **Native Importer/Exporter**: Core logic to transform Zustand store state to `.flowspace` JSON.
2. **mxGraph Parser**: Specialized service to traverse draw.io XML trees and map them to Node/Edge entities.
3. **Export Worker**: An asynchronous background job for generating heavy PDF/Image assets.
