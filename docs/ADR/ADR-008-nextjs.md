# ADR 008: Next.js 16 for Frontend Framework

## Status
Accepted

## Context
We need a frontend framework that provides excellent performance, developer experience, and support for modern web standards.

## Decision
We will use **Next.js 16** with the **App Router**. We will leverage Server Components for data fetching and Client Components for the interactive canvas.

## Alternatives
- **Vite + React**: Excellent for SPAs but lacks the built-in optimization and SSR capabilities of Next.js.
- **Remix**: A strong competitor, but Next.js has a larger ecosystem and better alignment with our target tech stack.

## Consequences
- **Pros**: Improved performance through Server Components, robust routing, and a massive ecosystem of plugins and tools.
- **Cons**: The App Router introduces new paradigms that require a mindset shift from traditional React SPAs.
