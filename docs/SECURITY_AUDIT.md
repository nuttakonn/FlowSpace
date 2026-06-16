# FlowSpace Security Audit Report

**Date**: June 2026
**Scope**: Full Stack (API, Persistence, Domain, Interop, AI)
**Status**: Resolved

This document outlines the findings of the final pre-production security audit. All critical and high-severity issues identified during this audit have been patched.

---

## 1. Executive Summary

The FlowSpace architecture leverages a robust Clean Architecture pattern with isolated Domain models and CQRS command handling. However, during the audit, several Missing Function Level Access Control (MFLAC) and Insecure Direct Object Reference (IDOR) vulnerabilities were identified at the controller level due to missing declarative authorization policies. Additionally, a cryptographic weakness was identified in the generation of public share links.

Both sets of issues were immediately remediated.

---

## 2. Findings and Remediations

### 2.1 Insecure Token Generation (High)
*   **Component**: `BoardShareLink.cs`
*   **Description**: Public share link tokens were being generated using `Guid.NewGuid().ToByteArray()`. While modern .NET utilizes a robust PRNG for GUIDs, it is not strictly defined as a Cryptographically Secure Pseudo-Random Number Generator (CSPRNG). In a highly sensitive context (Bearer tokens for public board access), this could theoretically lead to token predictability if the PRNG state is compromised.
*   **Remediation**: Refactored the `GenerateToken()` method to use `System.Security.Cryptography.RandomNumberGenerator.Fill()`. The tokens are now guaranteed to be 32 bytes of cryptographically secure randomness, Base64Url encoded.

### 2.2 Missing Function Level Access Control / IDOR (Critical)
*   **Component**: API Controllers (`InteropController`, `VersionsController`, `BoardsController`, `SharingController`, `WorkspacesController`, `WorkspaceMembersController`).
*   **Description**: While all endpoints required the user to be authenticated (`[Authorize]`), several read and mutation endpoints lacked explicit RBAC policy attributes (e.g., `[Authorize(Policy = Permissions.NodeRead)]`). Because the CQRS handlers did not always perform internal resource ownership checks, an authenticated attacker could access or modify boards/workspaces they did not belong to by guessing the target `Guid` (Insecure Direct Object Reference).
*   **Remediation**: Systematically audited all API controllers and applied explicit `[Authorize(Policy = ...)]` attributes. The `PermissionHandler` now intercepts every request, extracts the `workspaceId` or `boardId` from the route data, and executes a rigorous `IPermissionService` check against the database/cache before allowing the request to hit the CommandHandler.

---

## 3. Verified Protections (No Action Required)

The following areas were audited and found to be secure according to industry best practices:

*   **XML External Entity (XXE) Injection**: The `DrawIoConverter` strictly prohibits DTD processing (`DtdProcessing = DtdProcessing.Prohibit`). Verified by unit tests to block malicious `file:///` local read attempts and "Billion Laughs" XML expansion attacks.
*   **AI Prompt Injection & Schema Poisoning**: While prompt injection is inherently difficult to prevent at the LLM level, the system protects the database by routing all Gemini API output through `AiDiagramResponseValidator`. The validator enforces strict schema rules, strips unknown node types, and cross-references all edge definitions to ensure graph integrity before persistence.
*   **Information Leakage**: The custom `ExceptionHandlingMiddleware` correctly traps all unhandled server exceptions. It logs the stack trace to the internal Serilog sink but returns a generic, sanitized RFC 7807 `ProblemDetails` JSON response to the client, ensuring no internal infrastructure paths or database schema details are leaked.
*   **Database Injection**: The entire persistence layer utilizes Entity Framework Core with LINQ. No raw SQL concatenation is used, providing absolute protection against SQL Injection.

---

## 4. Ongoing Security Posture

- **Rate Limiting**: Configured globally via `AddFixedWindowLimiter` (100 req/10s).
- **Dependency Scanning**: Enforced via GitHub Actions (`npm audit` and `dotnet list package --vulnerable`).
- **Secrets Management**: No secrets are committed. Production utilizes `appsettings.Production.json` combined with Docker Secrets overlay `/run/secrets`.
