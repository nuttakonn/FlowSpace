"use client";

import { Component, ReactNode } from "react";
import { AlertTriangle, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  children?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class GlobalErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error("Uncaught error:", error, errorInfo);
    // In production, send this to an error tracking service (e.g., Sentry)
    // Sentry.captureException(error);
  }

  public render() {
    if (this.state.hasError) {
      return (
        <div className="flex h-screen w-full flex-col items-center justify-center bg-background p-6 text-center">
          <div className="mb-6 flex h-24 w-24 items-center justify-center rounded-full bg-destructive/10">
            <AlertTriangle className="h-12 w-12 text-destructive" />
          </div>
          <h1 className="mb-2 text-3xl font-black tracking-tight">Something went wrong</h1>
          <p className="mb-8 text-muted-foreground max-w-md">
            We've encountered an unexpected error. Our team has been notified.
            Please try refreshing the page.
          </p>
          <Button onClick={() => window.location.reload()} size="lg" className="gap-2 font-bold shadow-xl">
            <RefreshCw className="h-4 w-4" />
            Refresh Page
          </Button>
          
          {process.env.NODE_ENV === 'development' && this.state.error && (
            <div className="mt-8 p-4 bg-muted rounded text-left overflow-auto max-w-2xl max-h-64 text-xs font-mono text-muted-foreground">
               {this.state.error.stack}
            </div>
          )}
        </div>
      );
    }

    return this.props.children;
  }
}
