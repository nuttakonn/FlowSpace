# ADR 009: shadcn/ui for Component Library

## Status
Accepted

## Context
We need a consistent, accessible, and high-quality UI component library that allows for deep customization.

## Decision
We will use **shadcn/ui**. Unlike traditional component libraries, shadcn/ui provides components that are copied into our codebase, giving us full control.

## Alternatives
- **Material UI (MUI)**: Feature-rich but difficult to customize and adds significant bundle size.
- **Mantine**: Excellent hooks and components, but less flexible for a visual-heavy application like FlowSpace.

## Consequences
- **Pros**: Full ownership of component code, built on Radix UI (accessible), styled with Tailwind CSS, and extremely lightweight.
- **Cons**: Requires manual updates if the base components are improved in the upstream repository.
