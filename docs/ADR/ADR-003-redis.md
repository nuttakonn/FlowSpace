# ADR 003: Redis for Caching and Real-time State

## Status
Accepted

## Context
Real-time collaboration requires low-latency data access and a way to synchronize SignalR messages across multiple backend instances.

## Decision
We will use **Redis** for:
1. **Distributed Caching**: Frequently accessed workspace and user data.
2. **Backplane for SignalR**: Synchronizing messages across multiple server nodes.
3. **Presence Data**: Storing ephemeral data like active user lists and cursor positions.

## Alternatives
- **Memcached**: Limited to simple key-value pairs; lacks the data structures (Lists, Sets) and Pub/Sub features of Redis.
- **In-Memory Caching**: Does not scale across multiple server instances.

## Consequences
- **Pros**: Extremely low latency, versatile data structures, and acts as a foundation for scaling the real-time engine.
- **Cons**: Adds another infrastructure component to manage; data is ephemeral unless persistence is configured.
