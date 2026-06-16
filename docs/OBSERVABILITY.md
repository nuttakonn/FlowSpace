# Observability Documentation

## Pillars of Observability

### Logging
- **Standard**: Structured logging in JSON format.
- **Stack**: Serilog (Backend) -> ELK Stack (Elasticsearch, Logstash, Kibana) or Grafana Loki.
- **Context**: Every log must include `CorrelationId`, `UserId`, and `Environment`.

### Metrics
- **Standard**: Prometheus metrics.
- **Key Metrics**:
    - **API**: Request duration, 4xx/5xx rates.
    - **Real-time**: Active SignalR connections, message latency.
    - **System**: CPU/RAM usage, GC pressure.
    - **Business**: Boards created, AI requests processed.

### Tracing
- **Standard**: OpenTelemetry.
- **Tool**: Jaeger or Honeycomb.
- **Usage**: Trace requests from the Frontend -> API Gateway -> Backend -> Database/Redis to identify bottlenecks.

### Health Checks
- **Endpoints**: `/health/ready` and `/health/live`.
- **Backend**: ASP.NET Core Health Checks monitoring PostgreSQL and Redis connectivity.

## Dashboarding
- **Grafana**: Unified dashboards for infrastructure and application health.
- **Alerting**: PagerDuty or Slack notifications for critical failures (e.g., API 5xx spikes).
