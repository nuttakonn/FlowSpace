# Security Documentation

## Authentication
- **Provider**: JWT-based authentication.
- **Tokens**: Short-lived Access Tokens (15m) and long-lived Refresh Tokens (7 days) stored in HttpOnly cookies.
- **Identity**: ASP.NET Core Identity with custom User entity.

## Authorization
- **RBAC**: Role-Based Access Control for administrative tasks.
- **ABAC/Fine-grained**: Board-level permissions (Owner, Editor, Viewer) checked via specialized Authorization Handlers in MediatR behaviors.

## Data Protection
- **In-Transit**: Mandatory TLS 1.3 for all traffic.
- **At-Rest**: AES-256 encryption for sensitive database columns and MinIO object storage.
- **Secrets Management**: GitHub Actions Secrets for CI/CD; AWS Secrets Manager or HashiCorp Vault for Production.

## API Security
- **Rate Limiting**: Implemented at the API Gateway and application level (Redis-backed).
- **CORS**: Strict Origin validation (only allow `flowspace.app` and subdomains).
- **Input Validation**: Strict schema validation using FluentValidation on all API requests.

## Real-time Security
- **SignalR Hub Authorization**: Only authenticated users with 'Viewer' or 'Editor' access can join a board's SignalR group.
- **Yjs Sync**: Server-side validation of CRDT updates to prevent malicious node injection.
