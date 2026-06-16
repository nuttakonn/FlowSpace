# ADR 006: React Flow for Node-based Diagrams

## Status
Accepted

## Context
FlowSpace needs a powerful engine to render flowcharts, mindmaps, and other node-based process diagrams.

## Decision
We will use **React Flow** for the flowchart and mindmap modules. It provides a highly customizable, React-centric approach to node rendering and edge management.

## Alternatives
- **X6 (AntV)**: Very powerful but has a steeper learning curve and is less idiomatic for React developers.
- **GoJS**: Feature-rich but carries a heavy licensing cost and is not open-source.

## Consequences
- **Pros**: Fast development, excellent React integration, large community, and highly extensible via custom nodes/edges.
- **Cons**: Primarily focused on nodes and edges; may require custom logic for free-form drawing.
