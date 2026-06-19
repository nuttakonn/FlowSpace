# Testing Strategy

## Pyramid Approach
FlowSpace adheres to the testing pyramid, prioritizing fast unit tests while ensuring critical paths are covered by E2E tests.

## Unit Testing
- **Target**: Domain logic, Utility functions, MediatR Handlers.
- **Backend**: xUnit, FluentAssertions, Moq (for mocking).
- **Frontend**: Jest, React Testing Library.

## Integration Testing
- **Target**: Database repositories, API Endpoints, Redis integration.
- **Backend**: WebApplicationFactory with Testcontainers (PostgreSQL, Redis) to ensure real environment parity.
- **Frontend**: Mock Service Worker (MSW) to intercept API calls during component testing.

## E2E Testing
- **Target**: Critical user journeys (e.g., "Create Board", "AI Generate Diagram", "Real-time Collaboration").
- **Tools**: Playwright.
- **Execution**: Run against a transient Docker Compose environment in CI.

## Performance & Load Testing
- **Target**: SignalR hub capacity and Canvas rendering performance.
- **Tools**: k6 for API/SignalR load; Lighthouse/Web Vitals for frontend performance.

## CI/CD Integration
- Tests are executed on every Pull Request.
- **Threshold**: 80% code coverage required for core modules.
