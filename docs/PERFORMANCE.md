# Performance Documentation

## Frontend Performance
- **Rendering**: Heavy use of Next.js Server Components for initial load; Client Components for the interactive canvas.
- **Canvas Optimization**:
    - Use Layering (Background vs. Interactive nodes).
    - Implement Culling (only render nodes in the viewport).
    - Throttle/Debounce high-frequency events (cursor movements).
- **Assets**: Image optimization via `next/image`; SVG icons for scalability.

## Backend Performance
- **Asynchronous Execution**: All I/O operations must be `async/await`.
- **Caching**: 
    - Layer 1: In-memory (per instance).
    - Layer 2: Redis (distributed).
- **Database**:
    - Indexing on `WorkspaceId`, `BoardId`, and `OwnerId`.
    - Use `AsNoTracking()` in EF Core for read-only queries.
- **Pagination**: Mandatory for all list endpoints.

## Real-time Performance
- **Yjs**: CRDTs ensure conflict-free updates with minimal overhead.
- **SignalR**: Use Binary protocols (MessagePack) to reduce payload size.
- **Latency**: Targeted sub-200ms roundtrip for real-time collaboration.

## Scalability
- **Horizontal Scaling**: API instances scale based on CPU/Request count.
- **Redis Pub/Sub**: Used to synchronize SignalR messages across multiple backend instances.
