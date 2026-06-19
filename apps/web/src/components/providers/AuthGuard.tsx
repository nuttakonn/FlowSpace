"use client";

import { useAuthStore } from "@/store/useAuthStore";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

const authRoutes = ["/login", "/register"];
const publicRoutes = ["/"];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  const router = useRouter();
  const pathname = usePathname();
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    const isAuthRoute = authRoutes.includes(pathname);
    const urlParams = new URLSearchParams(window.location.search);
    const hasToken = urlParams.has("token");
    const isPublicRoute = publicRoutes.includes(pathname) || 
                          pathname.startsWith("/shared/") ||
                          (pathname.startsWith("/boards/") && hasToken);

    if (!isAuthenticated && !isAuthRoute && !isPublicRoute) {
      router.push("/login");
    } else if (isAuthenticated && isAuthRoute) {
      // Redirect logged-in users away from auth pages only
      router.push("/dashboard");
    } else {
      setIsReady(true);
    }
  }, [isAuthenticated, pathname, router]);

  if (!isReady) {
    return null; // Or a full page loading spinner
  }

  return <>{children}</>;
}
