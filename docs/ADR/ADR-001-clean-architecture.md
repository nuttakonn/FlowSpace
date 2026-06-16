# ADR 001: Clean Architecture

## Status
Accepted

## Context
FlowSpace requires a backend architecture that is maintainable, testable, and independent of external frameworks or databases. We need to support complex business logic for workspace management and real-time collaboration.

## Decision
We will adopt **Clean Architecture** (also known as Onion Architecture). The solution will be divided into four main layers:
1. **Domain**: Enterprise logic and entities (no dependencies).
2. **Application**: Use cases and MediatR handlers.
3. **Infrastructure**: External concerns like Database (EF Core), Redis, and File Storage.
4. **WebApi**: Entry point, Controllers, and SignalR Hubs.

## Alternatives
- **N-Tier Architecture**: Simpler but often leads to high coupling between business logic and the database.
- **Microservices**: Overkill for the initial phase; would increase operational complexity significantly.

## Consequences
- **Pros**: Highly testable logic, database independence, clear separation of concerns.
- **Cons**: Increased boilerplate (e.g., DTO mapping) and more projects/files to manage.
