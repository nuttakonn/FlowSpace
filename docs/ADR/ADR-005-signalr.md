# ADR 005: SignalR for Real-time Communication

## Status
Accepted

## Context
We need a robust, bi-directional communication channel between the client and the server to facilitate real-time updates.

## Decision
We will use **ASP.NET Core SignalR**. It will serve as the transport layer for Yjs updates and presence information.

## Alternatives
- **Raw WebSockets**: Requires manual handling of connection management, heartbeats, and fallbacks.
- **gRPC-Web**: Good for typed communication but less optimized for the broadcast patterns common in collaborative editing.

## Consequences
- **Pros**: Handles connection lifecycle, provides automatic fallbacks (Server-Sent Events, Long Polling), and integrates deeply with the ASP.NET Core ecosystem.
- **Cons**: Higher overhead than raw WebSockets; SignalR-specific client library required.
