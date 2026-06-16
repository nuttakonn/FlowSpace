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

- Clean Architecture
- CQRS with MediatR
- Real-time CRDTs with Yjs
