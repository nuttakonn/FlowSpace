# FlowSpace Post-Launch Beta Plan

This document outlines the strategy for managing the FlowSpace Beta Release, focusing on user feedback, system stability, and iterative quality improvements.

## 1. Beta Invitation Strategy

The goal of the Beta period is to expose the system to diverse real-world use cases while maintaining high operational performance.

- **Phase 1: Internal/Closed Beta**: Invite selected stakeholders and trusted partners (10-20 users).
- **Phase 2: Public Beta**: Open registration to a wider audience via social channels and the landing page (100+ users).
- **Onboarding Support**: Provide direct support via a dedicated Slack/Discord channel or an integrated feedback widget.

---

## 2. Feedback Collection Channels

### 2.1 Bug Reporting
- **In-App Feedback**: Utilize a lightweight reporting tool (e.g., a "Report Bug" button in the canvas header).
- **GitHub Issues**: Transition technical bugs to a private or public repository for tracking.
- **Categorization**: Priority 1 (Blocker), Priority 2 (Broken Feature), Priority 3 (UI/UX Glitch).

### 2.2 AI Quality Feedback
- **Implicit Feedback**: Track "Apply" vs. "Reject" rates for AI-generated previews via OpenTelemetry.
- **Explicit Feedback**: Prompt users with a simple "Was this diagram helpful? 👍/👎" after an AI generation session.
- **Prompt Refinement**: Log problematic prompts to improve the system instructions and few-shot examples.

### 2.3 Performance Monitoring
- **Real-time Latency**: Monitor SignalR message round-trip times and Yjs sync conflicts in production.
- **Canvas Scaling**: Observe UI responsiveness as board node counts approach the 10,000+ mark.
- **Export Latency**: Track Playwright rendering times for heavy diagrams.

---

## 3. Stabilization Roadmap (No Major Features)

The Beta period is strictly for **refining existing capabilities**. No new major modules will be added during this phase.

### Iteration 1: Stability & Security
- Patch all P1/P2 bugs identified during Phase 1.
- Refine Rate Limiting thresholds based on actual usage patterns.
- Enhance DB indexing for slow-running queries.

### Iteration 2: AI UX Polish
- Improve the "Diff Preview" rendering to handle complex overlaps better.
- Add more granular progress indicators for long AI generation tasks.
- Expand the built-in Template Marketplace with the most requested blueprints.

### Iteration 3: Performance Optimization
- Implement viewport culling improvements if 10k+ node boards show lag.
- Optimize Yjs binary updates to reduce WebSocket bandwidth consumption.
- Refine Playwright worker memory usage.

---

## 4. Success Metrics for GA (General Availability)

The transition from Beta to GA will be triggered when the following criteria are met:

1. **Uptime**: > 99.9% availability over a 30-day period.
2. **AI Accuracy**: > 85% "Apply" rate for first-time diagram generation.
3. **Performance**: LCP (Largest Contentful Paint) < 2.5s for initial board load.
4. **Reliability**: Zero P1 (Blocker) bugs remaining in the backlog.

---

**Plan Approved By:** _________________________  
**Target GA Date:** _________________________
