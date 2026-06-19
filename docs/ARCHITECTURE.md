# Architecture

## Overview

FlowSpace is built with a modern web architecture focusing on real-time collaboration and AI integration.

## Components

- **Frontend**: Next.js (React, TypeScript, Tailwind, Shadcn)
- **Backend**: ASP.NET Core 10 (MediatR, FluentValidation)
- **Real-time Engine**: Yjs and SignalR
- **Database**: PostgreSQL
- **Caching**: Redis
- **Storage**: MinIO

## Core Patterns

- Clean Architecture (Domain, Application, Infrastructure, Api)
- CQRS with MediatR for clean command/query handling
- Real-time CRDTs with Yjs and SignalR WebSockets transport
- Unified sidebar navigation shell (keeping users in-app between workspaces and the main dashboard)
- AI Assistant panels (triggerable from the editor header, sending prompts via MediatR to Gemini API services)
