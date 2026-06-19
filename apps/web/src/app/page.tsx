"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, Sparkles } from "lucide-react";
import { toast } from "sonner";

import { useAuthStore } from "@/store/useAuthStore";
import { apiClient } from "@/lib/api";

export default function HomePage() {
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  useEffect(() => {
    if (!isMounted) return;

    if (!isAuthenticated) {
      // Setup Guest Shadow Account Flow
      const setupShadowAccount = async () => {
        try {
          const shadowId = Math.random().toString(36).substring(2, 10);
          const shadowEmail = `guest-${shadowId}@flowspace.local`;
          const shadowPassword = `Shadow!${shadowId}Secure2026`;

          const response = await apiClient.post("/auth/register", {
            email: shadowEmail,
            password: shadowPassword,
            displayName: "Guest Creator",
          });

          useAuthStore.getState().setAuth(
            response.data.user,
            response.data.accessToken,
            response.data.refreshToken
          );
          
          toast.success("Guest workspace initialized");
          router.replace("/dashboard");
        } catch (error) {
          console.error("Failed to setup shadow account", error);
          toast.error("Failed to initialize session.");
        }
      };

      setupShadowAccount();
    } else {
      router.replace("/dashboard");
    }
  }, [isAuthenticated, isMounted, router]);

  return (
    <div className="flex h-screen items-center justify-center bg-background">
      <div className="flex flex-col items-center gap-4 text-center">
        <div className="bg-primary p-3 rounded-2xl animate-pulse shadow-lg shadow-primary/20">
          <Sparkles className="h-8 w-8 text-primary-foreground" />
        </div>
        <div className="space-y-1">
          <h1 className="text-xl font-bold tracking-tight">FlowSpace</h1>
          <p className="text-xs text-muted-foreground font-semibold flex items-center gap-1.5 justify-center">
            <Loader2 className="h-3 w-3 animate-spin text-primary" />
            Initializing workspace dashboard...
          </p>
        </div>
      </div>
    </div>
  );
}
