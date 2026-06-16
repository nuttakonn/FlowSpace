# ADR 002: PostgreSQL for Persistent Storage

## Status
Accepted

## Context
FlowSpace needs a reliable, ACID-compliant relational database to store user data, workspace metadata, permissions, and diagram structures.

## Decision
We will use **PostgreSQL** as our primary relational database. We will leverage its **JSONB** capabilities to store flexible diagram node metadata while keeping core relationships relational.

## Alternatives
- **MySQL**: Good performance but historically less robust JSON support compared to PostgreSQL.
- **MongoDB**: Excellent for flexible schemas but lacks the strong relational integrity needed for complex permissions and workspace hierarchies.

## Consequences
- **Pros**: Strong data integrity, excellent performance, rich feature set (JSONB, GIS, Full-text search), and strong ecosystem support.
- **Cons**: Requires careful indexing and schema management as the data grows.
