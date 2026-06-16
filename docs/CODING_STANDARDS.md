# Coding Standards

## General Principles
- **Clean Code**: Prioritize readability over cleverness.
- **DRY (Don't Repeat Yourself)**: Extract shared logic into `packages/`.
- **SOLID**: Strictly follow SOLID principles in the backend.

## Backend (C# / ASP.NET Core)
- **Style**: Follow Microsoft's C# Coding Conventions.
- **Architecture**: Use Clean Architecture (Domain -> Application -> Infrastructure -> WebApi).
- **CQRS**: All state changes must go through Commands; data retrieval through Queries.
- **Naming**: Use `PascalCase` for classes and methods; `camelCase` for private fields (with `_` prefix).

## Frontend (TypeScript / React)
- **Framework**: Next.js 16 (App Router).
- **Components**: Functional components only; use Shadcn UI for base primitives.
- **Styling**: Tailwind CSS with a mobile-first approach.
- **Types**: No `any`. Use strict TypeScript interfaces/types.
- **Naming**: `PascalCase` for components; `camelCase` for functions and variables.

## Documentation
- All public APIs must have XML/TSDoc comments.
- Update `docs/` for any significant architectural change.

## Tools
- **Linting**: ESLint (Frontend), StyleCop (Backend).
- **Formatting**: Prettier (Frontend), dotnet-format (Backend).
