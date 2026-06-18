import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { ThemeProvider } from "@/components/providers/ThemeProvider";
import { AuthProvider } from "@/components/providers/AuthProvider";
import { Toaster } from "@/components/ui/sonner";
import AuthGuard from "@/components/providers/AuthGuard";
import { GlobalErrorBoundary } from "@/components/monitoring/GlobalErrorBoundary";
import { WebVitals } from "@/components/monitoring/WebVitals";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "FlowSpace",
  description: "AI-native collaborative visual workspace platform",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        <style dangerouslySetInnerHTML={{
          __html: `
            nextjs-portal { display: none !important; }
            #webpack-hmr-error-overlay { display: none !important; }
          `
        }} />
        <script
          dangerouslySetInnerHTML={{
            __html: `
              const isResizeObserverError = (msg) => 
                msg && (
                  msg.includes('ResizeObserver loop limit exceeded') || 
                  msg.includes('ResizeObserver loop completed with undelivered notifications')
                );

              const originalError = console.error;
              console.error = (...args) => {
                if (args[0] && typeof args[0] === 'string' && isResizeObserverError(args[0])) return;
                originalError.apply(console, args);
              };

              window.addEventListener('error', function(e) {
                if (isResizeObserverError(e.message)) {
                  e.stopImmediatePropagation();
                  e.preventDefault();
                }
              }, true);

              window.addEventListener('unhandledrejection', function(e) {
                if (e.reason && isResizeObserverError(e.reason.message)) {
                  e.stopImmediatePropagation();
                  e.preventDefault();
                }
              }, true);
            `,
          }}
        />
      </head>
      <body className={inter.className}>
        <GlobalErrorBoundary>
          <WebVitals />
          <ThemeProvider
            attribute="class"
            defaultTheme="system"
            enableSystem
            disableTransitionOnChange
          >
            <AuthProvider>
              <AuthGuard>{children}</AuthGuard>
              <Toaster />
            </AuthProvider>
          </ThemeProvider>
        </GlobalErrorBoundary>
      </body>
    </html>
  );
}
