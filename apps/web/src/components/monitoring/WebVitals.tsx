"use client";

import { useReportWebVitals } from 'next/web-vitals';

export function WebVitals() {
  useReportWebVitals((metric) => {
    // In a real production app, this would be sent to an analytics endpoint (e.g., Datadog, Sentry, Google Analytics)
    // console.log(metric);
    if (metric.label === 'web-vital') {
       // example: send to custom api
       // fetch('/api/metrics', { body: JSON.stringify(metric), method: 'POST', keepalive: true });
    }
  });

  return null;
}
