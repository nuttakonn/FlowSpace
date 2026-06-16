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
