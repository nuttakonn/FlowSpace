"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Globe, Lock, ShieldCheck } from "lucide-react";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { FlowchartCanvas } from "@/components/canvas/FlowchartCanvas";
import { WhiteboardCanvas } from "@/components/canvas/WhiteboardCanvas";

interface Board {
  id: string;
  workspaceId: string;
  name: string;
  type: string;
}

export default function SharedBoardPage() {
  const params = useParams();
  const token = params.token as string;

  const [board, setBoard] = useState<Board | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSharedBoard = async () => {
      try {
        const response = await apiClient.get<Board>(`/shared/${token}`);
        setBoard(response.data);
      } catch (err: any) {
        setError(err.response?.data?.detail || "This link is invalid or has expired.");
      } finally {
        setIsLoading(false);
      }
    };

    if (token) fetchSharedBoard();
  }, [token]);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-10 w-10 animate-spin text-primary" />
      </div>
    );
  }

  if (error || !board) {
    return (
      <div className="flex h-screen flex-col items-center justify-center text-center p-6">
        <div className="h-20 w-20 rounded-full bg-destructive/10 flex items-center justify-center mb-6">
          <Lock className="h-10 w-10 text-destructive" />
        </div>
        <h2 className="text-2xl font-bold tracking-tight">Access Denied</h2>
        <p className="text-muted-foreground mt-2 max-w-md">{error}</p>
        <Button variant="outline" className="mt-8" onClick={() => window.location.href = "/dashboard"}>
          Return Home
        </Button>
      </div>
    );
  }

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-background">
      <header className="flex h-12 shrink-0 items-center justify-between border-b px-4 bg-muted/30 z-10">
        <div className="flex items-center gap-3">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary/10">
            <Globe className="h-4 w-4 text-primary" />
          </div>
          <div className="flex flex-col">
            <h1 className="text-xs font-bold leading-none">{board.name}</h1>
            <span className="text-[10px] text-muted-foreground font-medium">Shared via public link</span>
          </div>
        </div>
        <div className="flex items-center gap-2">
           <Badge variant="outline" className="text-[9px] font-black uppercase flex items-center gap-1">
             <ShieldCheck className="h-3 w-3" /> View Only
           </Badge>
        </div>
      </header>

      <main className="flex-1 relative overflow-hidden">
        {/* Note: In shared mode, we don't have a user, so we pass "Guest" and the token */}
        {board.type === "Whiteboard" ? (
          <WhiteboardCanvas boardId={board.id} workspaceId={board.workspaceId} accessToken="" userName="Guest" userId="00000000-0000-0000-0000-000000000000" token={token} />
        ) : (
          <FlowchartCanvas boardId={board.id} workspaceId={board.workspaceId} accessToken="" userName="Guest" userId="00000000-0000-0000-0000-000000000000" token={token} />
        )}
      </main>
    </div>
  );
}

import { Badge } from "@/components/ui/badge";
import { Loader2 } from "lucide-react";
