# ADR 010: ASP.NET Core 10 for Backend Framework

## Status
Accepted

## Context
The backend needs to be high-performance, type-safe, and capable of handling complex business logic and real-time connections.

## Decision
We will use **ASP.NET Core 10**. It provides the performance and enterprise-grade features required for FlowSpace.

## Alternatives
- **Node.js (Express/NestJS)**: Fast to develop but can become harder to maintain for very complex enterprise logic compared to C#.
- **Go**: Extremely performant but lacks the mature ecosystem for Clean Architecture and ORM features (like EF Core) that we prefer.

## Consequences
- **Pros**: Industry-leading performance, strong typing, excellent tooling (Visual Studio / JetBrains Rider), and a rich library ecosystem.
- **Cons**: Larger memory footprint than Go or Node.js; steeper learning curve for developers coming from dynamic languages.
