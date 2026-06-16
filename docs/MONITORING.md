# FlowSpace Production Monitoring

This document details the observability stack implemented to ensure visibility, performance tracking, and rapid error resolution in the production environment.

## 1. Backend: OpenTelemetry

The ASP.NET Core backend leverages the industry-standard **OpenTelemetry (OTel)** framework to provide distributed tracing and metrics.

### Configuration (`Program.cs`)
OpenTelemetry is configured to instrument three critical layers:
1.  **AspNetCore Instrumentation**: Captures incoming HTTP requests, route matching, and response times.
2.  **HttpClient Instrumentation**: Traces outbound requests, specifically calls to the Gemini API (`generativelanguage.googleapis.com`), allowing us to monitor AI response latency.
3.  **EntityFrameworkCore Instrumentation**: Traces database queries, ensuring slow SQL queries or N+1 issues are visible in APM tools.

**Exporter**: Data is currently configured to use `AddOtlpExporter()`, allowing traces and metrics to be seamlessly sent to an OpenTelemetry Collector or APM providers like Datadog, Honeycomb, or Jaeger without changing code.

### Custom Metrics
A dedicated meter (`FlowSpace.AI`) tracks the health of the AI integration:
-   **`ai.generation.attempts`**: A counter partitioned by `diagram_type` to monitor feature usage.
-   **`ai.generation.failures`**: A counter partitioned by `reason` (e.g., `api_error`, `empty_response`, `parse_error`) to trigger alerts if the Gemini API degrades or prompt schemas break.

---

## 2. Frontend: Next.js Performance & Errors

The React frontend utilizes a lightweight, native approach to track User Experience (UX) and runtime stability.

### Web Vitals (`WebVitals.tsx`)
Utilizes Next.js's `useReportWebVitals` hook. This captures Core Web Vitals (LCP, FID, CLS) and custom Next.js metrics (TTFB, FCP).
-   **Usage**: Currently logs to console in development. In production, uncomment the API fetch to push metrics to a dedicated analytics endpoint.

### Global Error Boundary (`GlobalErrorBoundary.tsx`)
Wraps the entire `RootLayout` to catch unhandled React render errors.
-   **UX Protection**: Prevents the "white screen of death" by displaying a polished fallback UI with a clear call-to-action to refresh.
-   **Tracking**: The `componentDidCatch` lifecycle method acts as the hook point for integration with tracking services like Sentry (`Sentry.captureException`).

---

## 3. Real-time Monitoring

### SignalR Hub
Real-time disconnects and reconnection attempts are natively logged by the ASP.NET Core `ILogger` within the SignalR pipeline. 
-   In the frontend `useCanvasStore`, connection failures specifically trigger user-facing `toast.error` notifications, but should be expanded in production to send a telemetry event if the failure rate exceeds expected thresholds.

## 4. Next Steps for DevOps
To activate the full potential of this implementation in your cloud environment:
1.  **Deploy an OTel Collector**: Run a sidecar or daemonset in your cluster to receive the OTLP data from the API.
2.  **Frontend SaaS**: Create a Sentry (or equivalent) project, add their snippet to `GlobalErrorBoundary`, and add your DSN to the frontend `.env`.
