"use client";

import { useAuthStore } from "@/store/useAuthStore";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

const publicRoutes = ["/login", "/register", "/"];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  const router = useRouter();
  const pathname = usePathname();
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    // Check if the current route is public
    const isPublicRoute = publicRoutes.includes(pathname);

    if (!isAuthenticated && !isPublicRoute) {
      router.push("/login");
    } else if (isAuthenticated && isPublicRoute && pathname !== "/") {
      // Redirect logged-in users away from auth pages
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
