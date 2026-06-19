"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, Sparkles, FolderPlus, LogIn, ArrowRight } from "lucide-react";
import { toast } from "sonner";

import { useAuthStore } from "@/store/useAuthStore";
import { apiClient } from "@/lib/api";

type Tab = "create" | "enter";

export default function HomePage() {
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();
  const [isMounted, setIsMounted] = useState(false);
  const [activeTab, setActiveTab] = useState<Tab>("create");
  
  // Create Workspace Form State
  const [createName, setCreateName] = useState("");
  const [isCreating, setIsCreating] = useState(false);
  const [createError, setCreateError] = useState("");

  // Enter Workspace Form State
  const [enterQuery, setEnterQuery] = useState("");
  const [isEntering, setIsEntering] = useState(false);
  const [enterError, setEnterError] = useState("");

  // Auth Initialization State
  const [isInitializingAuth, setIsInitializingAuth] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  useEffect(() => {
    if (!isMounted) return;

    if (!isAuthenticated) {
      setIsInitializingAuth(true);
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
        } catch (error) {
          console.error("Failed to setup shadow account", error);
        } finally {
          setIsInitializingAuth(false);
        }
      };

      setupShadowAccount();
    }
  }, [isAuthenticated, isMounted]);

  const handleCreateWorkspace = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!createName.trim()) return;
    setCreateError("");
    setIsCreating(true);

    try {
      // Ensure shadow account registration is completed
      if (isInitializingAuth && !isAuthenticated) {
        toast.info("Initializing session, please wait...");
        // Wait briefly
        await new Promise((resolve) => setTimeout(resolve, 1500));
        if (isInitializingAuth && !isAuthenticated) {
          throw new Error("Session initialization timeout. Please try again.");
        }
      }

      const res = await apiClient.post("/workspaces", { name: createName.trim() });
      toast.success("Workspace created successfully!");
      router.push(`/workspaces/${res.data.id}`);
    } catch (error: any) {
      console.error("Failed to create workspace", error);
      const backendMessage = error.response?.data?.detail;
      if (backendMessage && backendMessage.includes("exists")) {
        setCreateError("This workspace name/ID is already taken.");
      } else {
        setCreateError(backendMessage || "Failed to create workspace. Please try again.");
      }
      toast.error("Failed to create workspace");
    } finally {
      setIsCreating(false);
    }
  };

  const handleEnterWorkspace = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!enterQuery.trim()) return;
    setEnterError("");
    setIsEntering(true);

    try {
      // Ensure shadow account registration is completed
      if (isInitializingAuth && !isAuthenticated) {
        toast.info("Initializing session, please wait...");
        await new Promise((resolve) => setTimeout(resolve, 1500));
        if (isInitializingAuth && !isAuthenticated) {
          throw new Error("Session initialization timeout. Please try again.");
        }
      }

      const response = await apiClient.get(`/workspaces/lookup`, {
        params: { query: enterQuery.trim() }
      });
      
      toast.success(`Entering workspace: ${response.data.name}`);
      router.push(`/workspaces/${response.data.id}`);
    } catch (error: any) {
      console.error("Failed to look up workspace", error);
      setEnterError("Workspace not found. Check the name or ID.");
      toast.error("Workspace not found");
    } finally {
      setIsEntering(false);
    }
  };

  if (!isMounted) return null;

  return (
    <div className="relative min-h-screen w-full flex flex-col items-center justify-center bg-slate-950 text-slate-100 overflow-hidden font-sans select-none px-4 py-8">
      {/* Background Decorative Gradients */}
      <div className="absolute top-[-20%] left-[-20%] w-[60%] h-[60%] rounded-full bg-indigo-500/10 blur-[120px] pointer-events-none" />
      <div className="absolute bottom-[-20%] right-[-20%] w-[60%] h-[60%] rounded-full bg-violet-600/10 blur-[120px] pointer-events-none" />
      <div className="absolute top-[40%] left-[50%] -translate-x-1/2 -translate-y-1/2 w-[350px] h-[350px] rounded-full bg-indigo-500/5 blur-[80px] pointer-events-none" />

      <div className="w-full max-w-lg z-10 flex flex-col items-center">
        {/* Logo / Header */}
        <div className="flex items-center gap-2 mb-2 select-none">
          <div className="bg-gradient-to-tr from-indigo-500 to-violet-600 p-2.5 rounded-2xl shadow-xl shadow-indigo-500/15 animate-bounce">
            <Sparkles className="h-6 w-6 text-white" />
          </div>
          <span className="text-2xl font-black tracking-tight bg-gradient-to-r from-indigo-200 via-indigo-50 to-white bg-clip-text text-transparent">
            FlowSpace
          </span>
        </div>
        
        <p className="text-xs text-indigo-200/50 mb-8 font-medium tracking-wider uppercase">
          Collaborative Workspace Hub
        </p>

        {/* Main Glassmorphic Panel */}
        <div className="w-full rounded-3xl border border-white/10 bg-slate-900/60 backdrop-blur-xl shadow-2xl p-6 md:p-8 flex flex-col gap-6 relative overflow-hidden">
          
          {/* Tab Selector */}
          <div className="flex w-full bg-slate-950/80 p-1.5 rounded-2xl border border-white/[0.05]">
            <button
              onClick={() => { setActiveTab("create"); setCreateError(""); setEnterError(""); }}
              className={`flex-1 py-3 text-xs font-bold rounded-xl transition-all duration-200 flex items-center justify-center gap-2 ${
                activeTab === "create"
                  ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <FolderPlus className="h-4 w-4" />
              Create Workspace
            </button>
            <button
              onClick={() => { setActiveTab("enter"); setCreateError(""); setEnterError(""); }}
              className={`flex-1 py-3 text-xs font-bold rounded-xl transition-all duration-200 flex items-center justify-center gap-2 ${
                activeTab === "enter"
                  ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <LogIn className="h-4 w-4" />
              Enter Workspace
            </button>
          </div>

          {/* Create Workspace Panel */}
          {activeTab === "create" && (
            <form onSubmit={handleCreateWorkspace} className="flex flex-col gap-4">
              <div className="flex flex-col gap-2">
                <label className="text-[10px] font-black tracking-widest text-indigo-400 uppercase">
                  Workspace Name / ID
                </label>
                <div className="relative group">
                  <input
                    type="text"
                    required
                    placeholder="e.g. My Cool Team"
                    value={createName}
                    onChange={(e) => {
                      setCreateName(e.target.value);
                      if (createError) setCreateError("");
                    }}
                    className={`w-full px-4 py-3.5 bg-slate-950/80 border rounded-2xl text-sm font-semibold transition-all duration-200 text-white placeholder-slate-500 focus:outline-none focus:ring-2 ${
                      createError
                        ? "border-red-500/50 focus:ring-red-500/20 focus:border-red-500"
                        : "border-white/10 group-hover:border-white/20 focus:ring-indigo-500/20 focus:border-indigo-500"
                    }`}
                  />
                </div>
                {createError && (
                  <p className="text-xs font-bold text-red-400 mt-1 select-none">
                    {createError}
                  </p>
                )}
              </div>

              <button
                type="submit"
                disabled={isCreating || !createName.trim()}
                className="w-full mt-2 py-3.5 px-4 rounded-2xl bg-gradient-to-r from-indigo-600 to-violet-600 text-white hover:from-indigo-500 hover:to-violet-500 active:from-indigo-700 active:to-violet-700 disabled:opacity-50 disabled:cursor-not-allowed text-xs font-extrabold uppercase tracking-wider flex items-center justify-center gap-2 transition-all duration-200 shadow-lg shadow-indigo-600/10 hover:shadow-indigo-600/20"
              >
                {isCreating ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Creating Workspace...
                  </>
                ) : (
                  <>
                    Create Workspace
                    <ArrowRight className="h-4 w-4" />
                  </>
                )}
              </button>
            </form>
          )}

          {/* Enter Workspace Panel */}
          {activeTab === "enter" && (
            <form onSubmit={handleEnterWorkspace} className="flex flex-col gap-4">
              <div className="flex flex-col gap-2">
                <label className="text-[10px] font-black tracking-widest text-indigo-400 uppercase">
                  Workspace Name or ID
                </label>
                <div className="relative group">
                  <input
                    type="text"
                    required
                    placeholder="e.g. My Cool Team or UUID"
                    value={enterQuery}
                    onChange={(e) => {
                      setEnterQuery(e.target.value);
                      if (enterError) setEnterError("");
                    }}
                    className={`w-full px-4 py-3.5 bg-slate-950/80 border rounded-2xl text-sm font-semibold transition-all duration-200 text-white placeholder-slate-500 focus:outline-none focus:ring-2 ${
                      enterError
                        ? "border-red-500/50 focus:ring-red-500/20 focus:border-red-500"
                        : "border-white/10 group-hover:border-white/20 focus:ring-indigo-500/20 focus:border-indigo-500"
                    }`}
                  />
                </div>
                {enterError && (
                  <p className="text-xs font-bold text-red-400 mt-1 select-none">
                    {enterError}
                  </p>
                )}
              </div>

              <button
                type="submit"
                disabled={isEntering || !enterQuery.trim()}
                className="w-full mt-2 py-3.5 px-4 rounded-2xl bg-gradient-to-r from-indigo-600 to-violet-600 text-white hover:from-indigo-500 hover:to-violet-500 active:from-indigo-700 active:to-violet-700 disabled:opacity-50 disabled:cursor-not-allowed text-xs font-extrabold uppercase tracking-wider flex items-center justify-center gap-2 transition-all duration-200 shadow-lg shadow-indigo-600/10 hover:shadow-indigo-600/20"
              >
                {isEntering ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Searching Workspace...
                  </>
                ) : (
                  <>
                    Enter Workspace
                    <ArrowRight className="h-4 w-4" />
                  </>
                )}
              </button>
            </form>
          )}

          {/* Footnote Session Initializer Status */}
          {isInitializingAuth && (
            <div className="absolute bottom-2 left-0 right-0 text-center select-none">
              <span className="text-[9px] text-indigo-400/50 flex items-center justify-center gap-1.5 font-bold uppercase tracking-wider">
                <Loader2 className="h-3 w-3 animate-spin" />
                Initializing secure guest credentials...
              </span>
            </div>
          )}
        </div>

        {/* Aesthetic footer */}
        <div className="mt-8 text-center text-[10px] text-slate-500/80 font-bold uppercase tracking-widest flex items-center gap-2 select-none">
          <span>Secure Sandbox</span>
          <span className="h-1 w-1 rounded-full bg-slate-700" />
          <span>Real-time Sync</span>
        </div>
      </div>
    </div>
  );
}
