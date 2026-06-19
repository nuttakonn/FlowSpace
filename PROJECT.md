# FlowSpace

## Vision

FlowSpace is an AI-native collaborative visual workspace platform.

The platform combines:

* Whiteboard
* Flowchart
* Mindmap
* Wireframe
* AI Diagram Generation
* Real-time Collaboration

into a single unified experience.

---

## Business Goals

Provide a modern, personal, and team-focused alternative to:

* Whimsical
* Miro
* Draw.io
* FigJam

while integrating AI generation directly into the workflow. There is no plan to commercialize, sell, or operate FlowSpace as a SaaS; it is built purely as an internal utility for teams.

---

## Target Users

### Primary

* Software Engineers
* Architects
* Product Managers
* UX/UI Designers

### Secondary

* Students
* Small Teams
* Startups

---

## Core Modules

### Workspace

Multi-board organization.

### Whiteboard

Infinite canvas.

### Flowchart

Node-based process diagrams.

### Mindmap

Hierarchical visual thinking.

### Wireframe

Low fidelity interface design.

### AI Assistant

Generate diagrams from natural language.

### Collaboration

Real-time multi-user editing.

---

## Technical Stack

Frontend

* Next.js
* React
* TypeScript
* Tailwind
* Shadcn

Backend

* ASP.NET Core 10
* MediatR
* FluentValidation

Infrastructure

* PostgreSQL
* Redis
* MinIO

Realtime

* Yjs
* SignalR

Deployment

* Docker
* GitHub Actions

---

## Non Goals

Not included in v1:

* Video Calling
* Voice Chat
* Team Messaging
* Screen Sharing
* SaaS Billing & Pricing Plan Management (strictly personal & internal team edition)
* Favorites & Global App Settings (deferred/not included in initial version)

---

## Success Criteria

* Support 100 concurrent users
* Support 100,000 nodes per board
* Support realtime collaboration latency below 200ms
* Export PNG, PDF, SVG
* Generate diagrams through AI prompts (leveraging Gemini API)
* Unified sidebar layout navigation across Dashboard and Workspace pages
* AI Assistant trigger accessible via the Board editor header toolbar
