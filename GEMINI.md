# Gemini Global Instructions

You are working on FlowSpace.

Always follow these rules.

## Architecture

Respect layered architecture.

Frontend never accesses database.

Controllers never contain business logic.

Business logic belongs in Application layer.

Persistence belongs in Infrastructure layer.

---

## Backend Rules

Technology:

* ASP.NET Core 10
* CQRS
* MediatR
* FluentValidation

Folder Structure:

Features/
Commands/
Queries/
Validators/
Handlers/

Rules:

* One command per file
* One query per file
* One validator per file
* One handler per file

---

## Frontend Rules

Technology:

* Next.js App Router
* TypeScript Strict Mode
* Tailwind
* Shadcn

Rules:

* No inline styles
* No duplicated UI components
* Use reusable hooks

---

## TypeScript Rules

Forbidden:

* any
* ts-ignore

Required:

* explicit types
* strict mode

---

## Testing Rules

Every feature must include:

* unit tests
* integration tests

Coverage target:

* minimum 80%

---

## Security Rules

Always:

* validate input
* sanitize user content
* use parameterized queries

Never:

* trust frontend validation
* expose secrets
* hardcode credentials

---

## Performance Rules

Avoid:

* N+1 queries
* unnecessary re-renders

Prefer:

* pagination
* caching
* lazy loading

---

## Code Quality Rules

Maximum file size:

500 lines

Maximum function size:

100 lines

Maximum class size:

300 lines

Refactor when limits are exceeded.

---

## Output Rules

Generate production-ready code.

Do not generate pseudo code.

Do not generate placeholders.

Do not omit required files.

Generate complete implementations.
