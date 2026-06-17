"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Settings, ArrowLeft } from "lucide-react";
import Link from "next/link";

import { apiClient } from "@/lib/api";
import { useAuthStore } from "@/store/useAuthStore";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

import { FlowchartCanvas } from "@/components/canvas/FlowchartCanvas";
import { WhiteboardCanvas } from "@/components/canvas/WhiteboardCanvas";
import { ShareDialog } from "@/components/canvas/ShareDialog";

interface Board {
  id: string;
  workspaceId: string;
  name: string;
  type: string;
}

export default function BoardEditorPage() {
  const params = useParams();
  const boardId = params.boardId as string;
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);

  const [board, setBoard] = useState<Board | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchBoard = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const response = await apiClient.get<Board>(`/boards/${boardId}`);
        setBoard(response.data);
      } catch (err: unknown) {
        console.error("Failed to load board", err);
        const error = err as { response?: { status?: number } };
        if (error.response?.status === 401) {
          setError("Unauthorized: You don't have permission to view this board.");
        } else if (error.response?.status === 404 || error.response?.status === 400) {
          setError("Board not found.");
        } else {
          setError("An unexpected error occurred while loading the board.");
        }
      } finally {
        setIsLoading(false);
      }
    };

    if (boardId) fetchBoard();
  }, [boardId]);

  if (isLoading) {
    return (
      <div className="flex h-screen flex-col">
        <header className="flex h-14 items-center px-4 border-b bg-background gap-4">
          <Skeleton className="h-8 w-8 rounded-full" />
          <Skeleton className="h-6 w-32" />
        </header>
        <main className="flex-1 bg-muted/20 flex items-center justify-center">
          <Skeleton className="h-[80vh] w-[90vw] rounded-lg" />
        </main>
      </div>
    );
  }

  // Allow rendering if board is fetched, even if not logged in (to support share tokens)
  if (!board) {
    return (
      <div className="flex h-screen flex-col items-center justify-center text-center">
        <h2 className="text-2xl font-bold tracking-tight">{error || "Board not found"}</h2>
        <Link href="/" className="mt-4 text-primary hover:underline">Return to Home</Link>
      </div>
    );
  }

  const shareToken = typeof window !== 'undefined' ? new URLSearchParams(window.location.search).get('token') || undefined : undefined;

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-background">
      <header className="flex h-14 shrink-0 items-center justify-between border-b px-4 bg-background z-10">
        <div className="flex items-center gap-4">
          <Link href={`/workspaces/${board.workspaceId}`}>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div className="flex flex-col">
            <h1 className="text-sm font-semibold">{board.name}</h1>
            <span className="text-xs text-muted-foreground capitalize">{board.type}</span>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <ShareDialog boardId={board.id} />
          <Button variant="ghost" size="icon" className="h-8 w-8">
            <Settings className="h-4 w-4" />
          </Button>
        </div>
      </header>

      <main className="flex-1 relative overflow-hidden">
        <div className="absolute inset-0">
          {board.type === "Whiteboard" ? (
            <WhiteboardCanvas 
              boardId={board.id} 
              workspaceId={board.workspaceId} 
              accessToken={accessToken || ""} 
              userName={user?.displayName || "Guest"} 
              userId={user?.id || "guest"} 
              token={shareToken}
            />
          ) : (
            <FlowchartCanvas 
              boardId={board.id} 
              workspaceId={board.workspaceId} 
              accessToken={accessToken || ""} 
              userName={user?.displayName || "Guest"} 
              userId={user?.id || "guest"} 
              token={shareToken}
            />
          )}
        </div>
      </main>
    </div>
  );
}
