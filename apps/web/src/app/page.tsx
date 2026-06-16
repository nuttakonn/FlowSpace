"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { 
  Plus, Users, Layout, ArrowRight, Loader2, 
  Settings, Layers, Clock, Star
} from "lucide-react";
import { toast } from "sonner";

import { useAuthStore } from "@/store/useAuthStore";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Navbar } from "@/components/marketing/Navbar";

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

export default function HomePage() {
  const { user, isAuthenticated } = useAuthStore();
  
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [recentBoards, setRecentBoards] = useState<Board[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  useEffect(() => {
    if (!isMounted) return;

    if (!isAuthenticated) {
      router.push("/login");
      return;
    }

    if (isAuthenticated) {
      fetchDashboardData();
    }
  }, [isAuthenticated, isMounted, router]);

  const fetchDashboardData = async () => {
    try {
      setIsLoading(true);
      const wsResponse = await apiClient.get<Workspace[]>("/workspaces");
      setWorkspaces(wsResponse.data);
      
      // If there are workspaces, fetch boards for the first one as "recent" for now
      if (wsResponse.data.length > 0) {
        const boardsResponse = await apiClient.get<Board[]>(`/workspaces/${wsResponse.data[0].id}/boards`);
        setRecentBoards(boardsResponse.data.slice(0, 4));
      }
    } catch (error) {
      console.error("Failed to fetch dashboard data", error);
    } finally {
      setIsLoading(false);
    }
  };

  if (!isMounted || (isAuthenticated && isLoading)) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!isAuthenticated) return null;

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <Navbar />
      
      <main className="container mx-auto flex-1 p-6 space-y-8">
        {/* Welcome Section */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b pb-8">
          <div>
            <h1 className="text-4xl font-black tracking-tighter">Welcome, {user?.displayName || "Member"}</h1>
            <p className="text-muted-foreground font-medium mt-1">
              What are we building today?
            </p>
          </div>
          <div className="flex gap-3">
            <Button size="lg" className="font-bold shadow-lg shadow-primary/20" asChild>
              <Link href="/workspaces">
                <Plus className="mr-2 h-5 w-5" /> New Workspace
              </Link>
            </Button>
          </div>
        </div>

        <div className="grid gap-8 md:grid-cols-3">
          {/* Workspaces Section */}
          <div className="md:col-span-2 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold tracking-tight flex items-center gap-2">
                <Users className="h-5 w-5 text-primary" /> 
                My Workspaces
              </h2>
              <Button variant="ghost" size="sm" className="text-xs font-bold uppercase tracking-wider" asChild>
                <Link href="/workspaces">View All</Link>
              </Button>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              {workspaces.length === 0 ? (
                <Card className="border-dashed flex flex-col items-center justify-center p-8 text-center bg-muted/30">
                  <div className="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center mb-4">
                    <Users className="h-6 w-6 text-primary" />
                  </div>
                  <CardTitle className="text-lg">No Workspaces</CardTitle>
                  <CardDescription className="mt-2">
                    Create a workspace to start organizing your boards and team.
                  </CardDescription>
                  <Button variant="outline" size="sm" className="mt-4 font-bold" asChild>
                    <Link href="/workspaces">Create First Workspace</Link>
                  </Button>
                </Card>
              ) : (
                workspaces.slice(0, 4).map((ws) => (
                  <Card key={ws.id} className="group hover:border-primary/50 transition-all cursor-pointer relative overflow-hidden">
                    <Link href={`/workspaces/${ws.id}`} className="absolute inset-0 z-0" />
                    <CardHeader className="pb-2">
                      <CardTitle className="text-lg group-hover:text-primary transition-colors">{ws.name}</CardTitle>
                      <CardDescription>Created {new Date(ws.createdAt).toLocaleDateString()}</CardDescription>
                    </CardHeader>
                    <CardContent className="pt-0">
                      <div className="flex items-center gap-1 text-[10px] font-black uppercase text-muted-foreground tracking-widest">
                        <Layers className="h-3 w-3" /> Active Workspace
                      </div>
                    </CardContent>
                  </Card>
                ))
              )}
            </div>
          </div>

          {/* Quick Stats / Recent Boards */}
          <div className="space-y-4">
            <h2 className="text-xl font-bold tracking-tight flex items-center gap-2">
              <Clock className="h-5 w-5 text-primary" />
              Recent Boards
            </h2>
            
            <div className="space-y-3">
              {recentBoards.length === 0 ? (
                <div className="text-sm text-muted-foreground bg-muted/20 rounded-xl p-6 border border-dashed text-center">
                  Select a workspace to see your boards.
                </div>
              ) : (
                recentBoards.map((board) => (
                  <Link key={board.id} href={`/boards/${board.id}`}>
                    <div className="flex items-center gap-3 p-3 rounded-xl border hover:bg-muted/50 transition-colors group">
                      <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                        <Layout className="h-5 w-5 text-primary" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-bold truncate group-hover:text-primary transition-colors">{board.name}</p>
                        <p className="text-[10px] text-muted-foreground uppercase font-black">{board.type}</p>
                      </div>
                      <ArrowRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" />
                    </div>
                  </Link>
                ))
              )}
              
              <div className="pt-4 border-t">
                 <Button variant="outline" className="w-full font-bold" asChild>
                    <Link href="/workspaces">
                       Manage All Boards
                    </Link>
                 </Button>
              </div>
            </div>
          </div>
        </div>

        {/* Shortcuts Section */}
        <div className="grid gap-6 md:grid-cols-4 pt-4">
           <Card className="bg-primary text-primary-foreground border-none shadow-xl shadow-primary/20">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-black uppercase tracking-tighter opacity-80">Quick Create</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-xl font-bold leading-tight">New Flowchart</p>
                <Button variant="secondary" size="sm" className="mt-4 w-full font-bold group" asChild>
                   <Link href="/workspaces">
                      Choose Workspace <ArrowRight className="ml-2 h-4 w-4 group-hover:translate-x-1 transition-transform" />
                   </Link>
                </Button>
              </CardContent>
           </Card>

           <Card className="bg-zinc-900 text-zinc-100 border-none">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-black uppercase tracking-tighter opacity-80">Collaborate</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-xl font-bold leading-tight">Shared Links</p>
                <Button variant="ghost" size="sm" className="mt-4 w-full font-bold hover:bg-white/10" asChild>
                   <Link href="/workspaces">
                      View Shared Assets
                   </Link>
                </Button>
              </CardContent>
           </Card>

           <Card className="border-2 border-dashed border-muted flex items-center justify-center p-6 grayscale opacity-60 hover:grayscale-0 hover:opacity-100 transition-all cursor-not-allowed">
              <div className="text-center">
                 <Star className="h-6 w-6 mx-auto mb-2 text-yellow-500" />
                 <p className="text-xs font-black uppercase tracking-widest">Favorites</p>
                 <p className="text-[10px] mt-1 font-medium italic">Coming Soon</p>
              </div>
           </Card>

           <Card className="border-2 border-dashed border-muted flex items-center justify-center p-6 grayscale opacity-60 hover:grayscale-0 hover:opacity-100 transition-all cursor-not-allowed">
              <div className="text-center">
                 <Settings className="h-6 w-6 mx-auto mb-2 text-primary" />
                 <p className="text-xs font-black uppercase tracking-widest">Settings</p>
                 <p className="text-[10px] mt-1 font-medium italic">Coming Soon</p>
              </div>
           </Card>
        </div>
      </main>

      <footer className="border-t py-6 mt-12 bg-muted/10">
        <div className="container mx-auto px-6 flex justify-between items-center">
           <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest">FlowSpace Personal / Team Edition</p>
           <p className="text-[10px] font-medium text-muted-foreground">© 2026</p>
        </div>
      </footer>
    </div>
  );
}
