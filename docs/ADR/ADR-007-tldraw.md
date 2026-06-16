# ADR 007: tldraw for Whiteboard and Free-form Drawing

## Status
Accepted

## Context
In addition to structured diagrams, users need a "whiteboard" experience for free-form drawing, sketching, and sticky notes.

## Decision
We will use **tldraw** as the engine for the Whiteboard and Wireframe modules. It provides a modern, high-performance drawing experience with a unique aesthetic.

## Alternatives
- **Fabric.js**: Powerful for canvas manipulation but feels dated and is harder to integrate with modern React state management.
- **Konva.js**: Excellent for 2D graphics but requires significant effort to build a full "whiteboard" interface from scratch.

## Consequences
- **Pros**: "Out-of-the-box" whiteboard features, great UX, and active development.
- **Cons**: The library is evolving rapidly; APIs may change. Integrating deeply with Yjs requires careful implementation.
