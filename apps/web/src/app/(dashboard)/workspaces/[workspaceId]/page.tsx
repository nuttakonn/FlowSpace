"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { 
  Layout, Plus, Settings, MoreVertical, Pencil, Trash2, Loader2
} from "lucide-react";

import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardDescription, CardHeader, CardTitle, CardFooter } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbSeparator } from "@/components/ui/breadcrumb";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { TemplateGallery } from "@/components/canvas/TemplateGallery";

interface Board {
  id: string;
  workspaceId: string;
  name: string;
  type: string;
  createdAt: string;
}

interface Workspace {
  id: string;
  name: string;
  ownerId: string;
}

const BOARD_TYPES = ["Whiteboard", "Flowchart", "Mindmap", "Wireframe"] as const;

const createBoardSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name is too long"),
  type: z.enum(BOARD_TYPES),
});

const renameBoardSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name is too long"),
});

type CreateBoardFormValues = z.infer<typeof createBoardSchema>;
type RenameBoardFormValues = z.infer<typeof renameBoardSchema>;

export default function WorkspaceDetailsPage() {
  const params = useParams();
  const router = useRouter();
  const workspaceId = params.workspaceId as string;

  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  const [boards, setBoards] = useState<Board[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Dialog States
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isRenameOpen, setIsRenameOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [activeBoard, setActiveBoard] = useState<Board | null>(null);
  const [isTemplateGalleryOpen, setIsTemplateGalleryOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  // Forms
  const renameForm = useForm<RenameBoardFormValues>({
    resolver: zodResolver(renameBoardSchema),
    defaultValues: { name: "" },
  });

  const fetchData = async (id: string) => {
    try {
      setIsLoading(true);
      const [wsResponse, boardsResponse] = await Promise.all([
        apiClient.get<Workspace>(`/workspaces/${id}`),
        apiClient.get<Board[]>(`/workspaces/${id}/boards`),
      ]);
      
      setWorkspace(wsResponse.data);
      setBoards(boardsResponse.data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to load workspace details");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (workspaceId) {
      fetchData(workspaceId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [workspaceId]);

  // Handlers
  const handleCreate = async (template: any, name: string) => {
    try {
      setIsCreating(true);
      const response = await apiClient.post<Board>(`/workspaces/${workspaceId}/boards`, { 
        name: name,
        type: template.boardType 
      });
      const newBoard = response.data;

      // If it's a specific template (not blank), we should ideally call a backend endpoint to generate/apply it.
      // Since the backend template logic might need a dedicated endpoint, we'll apply it via the AI/Template service or redirect immediately.
      // For now, redirecting to the new board is the expected behavior.
      
      toast.success("Board created successfully");
      setIsTemplateGalleryOpen(false);
      
      // Redirect to the newly created board
      router.push(`/boards/${newBoard.id}`);
      
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to create board");
      setIsCreating(false); // Only set false on error, let the redirect handle success state
    }
  };

  const openRename = (board: Board) => {
    setActiveBoard(board);
    renameForm.setValue("name", board.name);
    setIsRenameOpen(true);
  };

  const handleRename = async (data: RenameBoardFormValues) => {
    if (!activeBoard) return;
    try {
      await apiClient.put(`/boards/${activeBoard.id}`, { name: data.name });
      toast.success("Board renamed successfully");
      setIsRenameOpen(false);
      
      // Refresh board list
      const boardsResponse = await apiClient.get<Board[]>(`/workspaces/${workspaceId}/boards`);
      setBoards(boardsResponse.data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to rename board");
    }
  };

  const openDelete = (board: Board) => {
    setActiveBoard(board);
    setIsDeleteOpen(true);
  };

  const handleDelete = async () => {
    if (!activeBoard) return;
    try {
      await apiClient.delete(`/boards/${activeBoard.id}`);
      toast.success("Board deleted successfully");
      setIsDeleteOpen(false);
      
      // Refresh board list
      const boardsResponse = await apiClient.get<Board[]>(`/workspaces/${workspaceId}/boards`);
      setBoards(boardsResponse.data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to delete board");
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-10 w-full max-w-sm" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      </div>
    );
  }

  if (!workspace) {
    return (
      <div className="flex h-[50vh] flex-col items-center justify-center text-center">
        <h2 className="text-2xl font-bold tracking-tight">Workspace not found</h2>
        <Link href="/workspaces" className="mt-4 text-primary hover:underline">Return to Workspaces</Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink href="/workspaces">Workspaces</BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbLink href={`/workspaces/${workspaceId}`}>{workspace.name}</BreadcrumbLink>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <div className="flex flex-col sm:flex-row gap-4 sm:items-center justify-between">
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-3xl font-bold tracking-tight">{workspace.name}</h1>
          </div>
          <p className="text-muted-foreground">Manage your boards and collaborators.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Settings className="mr-2 h-4 w-4" /> Workspace Settings
          </Button>
          <Button onClick={() => setIsTemplateGalleryOpen(true)}>
            <Plus className="mr-2 h-4 w-4" /> New Board
          </Button>
        </div>
      </div>

      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold tracking-tight">Boards</h2>
        </div>

        {boards.length === 0 ? (
          <div className="flex h-40 flex-col items-center justify-center gap-4 rounded-lg border border-dashed bg-muted/30 text-center p-8">
            <div className="rounded-full bg-primary/10 p-4">
              <Layout className="h-8 w-8 text-primary" />
            </div>
            <div className="space-y-1">
              <h3 className="font-medium text-lg">No boards created yet</h3>
              <p className="text-sm text-muted-foreground">Get started by creating your first visual board.</p>
            </div>
            <Button onClick={() => setIsTemplateGalleryOpen(true)} className="mt-2">
              <Plus className="mr-2 h-4 w-4" /> Create Board
            </Button>
          </div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {boards.map((board) => (
              <Card key={board.id} className="group relative hover:border-primary/50 transition-colors flex flex-col h-full">
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div>
                      <CardTitle className="text-lg line-clamp-1 pr-6" title={board.name}>
                        <Link href={`/boards/${board.id}`} className="hover:underline">
                          {board.name}
                        </Link>
                      </CardTitle>
                      <CardDescription className="capitalize mt-1">
                        {board.type}
                      </CardDescription>
                    </div>
                    
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="h-8 w-8 p-0 absolute top-4 right-4 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 focus:opacity-100 transition-opacity">
                          <span className="sr-only">Open menu</span>
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => openRename(board)}>
                          <Pencil className="mr-2 h-4 w-4" /> Rename
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => openDelete(board)} className="text-destructive focus:text-destructive">
                          <Trash2 className="mr-2 h-4 w-4" /> Delete
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>
                </CardHeader>
                <div className="flex-1" />
                <CardFooter className="pt-4 pb-4">
                  <Button variant="secondary" className="w-full" asChild>
                    <Link href={`/boards/${board.id}`}>Open Board</Link>
                  </Button>
                </CardFooter>
              </Card>
            ))}
          </div>
        )}
      </div>

      <TemplateGallery 
        isOpen={isTemplateGalleryOpen} 
        onOpenChange={setIsTemplateGalleryOpen} 
        onSelect={handleCreate}
        isSubmitting={isCreating}
      />

      {/* Rename Dialog */}
      <Dialog open={isRenameOpen} onOpenChange={setIsRenameOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Rename Board</DialogTitle>
            <DialogDescription>Change the name of {activeBoard?.name}.</DialogDescription>
          </DialogHeader>
          <form onSubmit={renameForm.handleSubmit(handleRename)}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="rename-name">New Name</Label>
                <Input id="rename-name" {...renameForm.register("name")} />
                {renameForm.formState.errors.name && (
                  <p className="text-sm text-destructive">{renameForm.formState.errors.name.message}</p>
                )}
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsRenameOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={renameForm.formState.isSubmitting}>
                {renameForm.formState.isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save Changes
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Alert Dialog */}
      <AlertDialog open={isDeleteOpen} onOpenChange={setIsDeleteOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Board</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete &quot;{activeBoard?.name}&quot;? All nodes and visual data will be permanently removed. This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Delete Board
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

    </div>
  );
}
