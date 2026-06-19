"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { 
  Plus, Users, Layout, ArrowRight, Loader2, 
  Layers, Clock, Sparkles, FolderPlus, FilePlus, ChevronRight
} from "lucide-react";
import { toast } from "sonner";

import { useAuthStore } from "@/store/useAuthStore";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

interface Workspace {
  id: string;
  name: string;
  createdAt: string;
}

interface Board {
  id: string;
  workspaceId: string;
  name: string;
  type: string;
  updatedAt?: string;
}

export default function DashboardPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [recentBoards, setRecentBoards] = useState<Board[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreateWorkspaceOpen, setIsCreateWorkspaceOpen] = useState(false);
  const [newWorkspaceName, setNewWorkspaceName] = useState("");
  const [isCreatingWorkspace, setIsCreatingWorkspace] = useState(false);

  const fetchDashboardData = async () => {
    try {
      setIsLoading(true);
      const wsResponse = await apiClient.get<Workspace[]>("/workspaces");
      setWorkspaces(wsResponse.data);
      
      if (wsResponse.data.length > 0) {
        // Fetch boards for all workspaces to get a true representation of recent boards
        const allBoardsPromises = wsResponse.data.map(ws => 
          apiClient.get<Board[]>(`/workspaces/${ws.id}/boards`)
            .catch(() => ({ data: [] as Board[] }))
        );
        const boardsResults = await Promise.all(allBoardsPromises);
        const combinedBoards = boardsResults.flatMap(res => res.data);
        
        // Sort by updatedAt or createdAt if available, otherwise just slice
        setRecentBoards(combinedBoards.slice(0, 5));
      } else {
        setRecentBoards([]);
      }
    } catch (error) {
      console.error("Failed to fetch dashboard data", error);
      toast.error("Failed to sync dashboard.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      fetchDashboardData();
    } else {
      router.replace("/");
    }
  }, [isAuthenticated]);

  const handleCreateWorkspace = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newWorkspaceName.trim()) return;

    try {
      setIsCreatingWorkspace(true);
      const res = await apiClient.post<Workspace>("/workspaces", { name: newWorkspaceName.trim() });
      toast.success("Workspace created successfully");
      setNewWorkspaceName("");
      setIsCreateWorkspaceOpen(false);
      fetchDashboardData();
      router.push(`/workspaces/${res.data.id}`);
    } catch (error) {
      console.error("Failed to create workspace", error);
      toast.error("Failed to create workspace");
    } finally {
      setIsCreatingWorkspace(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-[70vh] items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm font-semibold text-muted-foreground animate-pulse">Syncing visual workspace...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto space-y-8 p-1">
      {/* Premium Hero Panel */}
      <div className="relative overflow-hidden rounded-3xl bg-gradient-to-r from-violet-600 via-indigo-600 to-blue-600 p-8 text-white shadow-xl shadow-indigo-500/10">
        <div className="absolute -right-10 -top-10 h-40 w-40 rounded-full bg-white/10 blur-2xl"></div>
        <div className="absolute -left-10 -bottom-10 h-40 w-40 rounded-full bg-white/10 blur-2xl"></div>
        
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="space-y-2">
            <div className="inline-flex items-center gap-1.5 rounded-full bg-white/15 px-3 py-1 text-xs font-semibold backdrop-blur-md">
              <Sparkles className="h-3 w-3 text-amber-300" />
              <span>Personal & Team Editor</span>
            </div>
            <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight">
              Welcome back, {user?.displayName || "Member"}
            </h1>
            <p className="text-white/80 text-sm md:text-base max-w-xl font-medium">
              Create, visual-model, and co-design architectures. Your design studio is primed.
            </p>
          </div>
          <div className="flex flex-wrap gap-3 shrink-0">
            <Button 
              size="lg" 
              variant="secondary" 
              className="font-bold shadow-lg"
              onClick={() => setIsCreateWorkspaceOpen(true)}
            >
              <FolderPlus className="mr-2 h-5 w-5" /> New Workspace
            </Button>
          </div>
        </div>
      </div>

      {/* Metrics Row */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card className="border-none bg-card shadow-sm hover:shadow-md transition-all">
          <CardContent className="p-6 flex items-center gap-4">
            <div className="h-12 w-12 rounded-2xl bg-violet-500/10 text-violet-500 flex items-center justify-center shrink-0">
              <Users className="h-6 w-6" />
            </div>
            <div>
              <p className="text-xs text-muted-foreground font-black uppercase tracking-widest">Workspaces</p>
              <h3 className="text-2xl font-bold mt-0.5">{workspaces.length}</h3>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none bg-card shadow-sm hover:shadow-md transition-all">
          <CardContent className="p-6 flex items-center gap-4">
            <div className="h-12 w-12 rounded-2xl bg-blue-500/10 text-blue-500 flex items-center justify-center shrink-0">
              <Layout className="h-6 w-6" />
            </div>
            <div>
              <p className="text-xs text-muted-foreground font-black uppercase tracking-widest">Active Boards</p>
              <h3 className="text-2xl font-bold mt-0.5">{recentBoards.length}</h3>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-8 lg:grid-cols-3">
        {/* Workspaces List Section */}
        <div className="lg:col-span-2 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold tracking-tight flex items-center gap-2">
              <Users className="h-5 w-5 text-indigo-500" /> 
              My Workspaces
            </h2>
            <Button variant="ghost" size="sm" className="text-xs font-bold uppercase tracking-wider text-indigo-600 hover:text-indigo-700" asChild>
              <Link href="/workspaces">View All <ChevronRight className="h-4 w-4 ml-0.5" /></Link>
            </Button>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {workspaces.length === 0 ? (
              <Card className="border-dashed col-span-full flex flex-col items-center justify-center p-12 text-center bg-muted/20">
                <div className="h-16 w-16 rounded-3xl bg-indigo-500/5 flex items-center justify-center mb-4">
                  <Users className="h-8 w-8 text-indigo-400" />
                </div>
                <CardTitle className="text-lg font-bold">No Workspaces Yet</CardTitle>
                <CardDescription className="mt-2 max-w-xs mx-auto">
                  Create a collaborative workspace to begin structuring your architecture maps and documents.
                </CardDescription>
                <Button 
                  onClick={() => setIsCreateWorkspaceOpen(true)}
                  className="mt-6 font-bold bg-indigo-600 hover:bg-indigo-700 shadow-md"
                >
                  <Plus className="mr-2 h-4 w-4" /> Create First Workspace
                </Button>
              </Card>
            ) : (
              workspaces.slice(0, 4).map((ws) => (
                <Card key={ws.id} className="group border-none bg-card hover:bg-slate-50/50 dark:hover:bg-slate-900/50 hover:shadow-lg transition-all duration-300 relative overflow-hidden flex flex-col justify-between">
                  <Link href={`/workspaces/${ws.id}`} className="absolute inset-0 z-0" />
                  <CardHeader className="pb-4">
                    <CardTitle className="text-lg font-bold group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors truncate">{ws.name}</CardTitle>
                    <CardDescription className="text-xs">Created {new Date(ws.createdAt).toLocaleDateString()}</CardDescription>
                  </CardHeader>
                  <CardContent className="pt-0 pb-6 flex justify-between items-center z-10">
                    <div className="flex items-center gap-1.5 text-[10px] font-black uppercase text-muted-foreground tracking-widest bg-muted/60 px-2 py-0.5 rounded">
                      <Layers className="h-3 w-3" /> Active Workspace
                    </div>
                    <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:translate-x-1 transition-transform" />
                  </CardContent>
                </Card>
              ))
            )}
          </div>
        </div>

        {/* Recent Boards Section */}
        <div className="space-y-4">
          <h2 className="text-xl font-bold tracking-tight flex items-center gap-2">
            <Clock className="h-5 w-5 text-indigo-500" />
            Recent Boards
          </h2>
          
          <div className="space-y-3">
            {recentBoards.length === 0 ? (
              <div className="text-sm text-muted-foreground bg-muted/20 rounded-2xl p-8 border border-dashed text-center">
                Select or create a workspace to begin mapping visual boards.
              </div>
            ) : (
              recentBoards.map((board) => (
                <Link key={board.id} href={`/boards/${board.id}`}>
                  <div className="flex items-center gap-3 p-3.5 rounded-2xl border-none bg-card hover:shadow-md transition-all group cursor-pointer">
                    <div className="h-10 w-10 rounded-xl bg-indigo-500/10 flex items-center justify-center shrink-0 text-indigo-600">
                      <Layout className="h-5 w-5" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-bold truncate group-hover:text-indigo-600 transition-colors">{board.name}</p>
                      <p className="text-[10px] text-muted-foreground uppercase font-black tracking-wider">{board.type}</p>
                    </div>
                    <ArrowRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 group-hover:translate-x-1 transition-all" />
                  </div>
                </Link>
              ))
            )}
            
            <div className="pt-4">
               <Button variant="outline" className="w-full font-bold border-2 hover:bg-slate-50 dark:hover:bg-slate-900" asChild>
                  <Link href="/workspaces">
                     Manage All Boards
                  </Link>
               </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Dialog for creating a new Workspace */}
      <Dialog open={isCreateWorkspaceOpen} onOpenChange={setIsCreateWorkspaceOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <form onSubmit={handleCreateWorkspace}>
            <DialogHeader>
              <DialogTitle>Create Workspace</DialogTitle>
              <DialogDescription>
                Add a new workspace to organize your design architecture boards and teammates.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name" className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Workspace Name</Label>
                <Input
                  id="name"
                  placeholder="e.g., Cloud Infrastructure Team"
                  value={newWorkspaceName}
                  onChange={(e) => setNewWorkspaceName(e.target.value)}
                  className="col-span-3"
                  required
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsCreateWorkspaceOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" className="bg-indigo-600 hover:bg-indigo-700 text-white" disabled={isCreatingWorkspace}>
                {isCreatingWorkspace && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create Workspace
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
