# ADR 004: Yjs for Shared State Consistency

## Status
Accepted

## Context
A collaborative visual workspace requires multiple users to edit the same diagram simultaneously without conflicts.

## Decision
We will use **Yjs** as our Conflict-free Replicated Data Type (CRDT) engine. Yjs will manage the shared state of the canvas, ensuring that all clients eventually converge on the same state.

## Alternatives
- **Operational Transformation (OT)**: Used by Google Docs; extremely complex to implement for non-textual data like diagrams.
- **Automerge**: Another CRDT library; while robust, Yjs currently offers better performance for large visual datasets.

## Consequences
- **Pros**: Automatic conflict resolution, offline support, and high performance during real-time synchronization.
- **Cons**: Steeper learning curve; requires careful integration with the rendering engine (React Flow/tldraw).
