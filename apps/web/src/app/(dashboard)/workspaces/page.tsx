"use client";

import { useEffect, useState, useMemo } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { 
  Plus, Users, Search, MoreVertical, Pencil, Trash2, Loader2
} from "lucide-react";

import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardDescription, CardHeader, CardTitle, CardFooter } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
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
  Pagination,
  PaginationContent,
  PaginationItem,
} from "@/components/ui/pagination";
import { ResponsiveDialog } from "@/components/ui/responsive-dialog";
import { Label } from "@/components/ui/label";

interface Workspace {
  id: string;
  name: string;
  ownerId: string;
  createdAt: string;
}

const workspaceSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name is too long"),
});

type WorkspaceFormValues = z.infer<typeof workspaceSchema>;

const ITEMS_PER_PAGE = 6;

export default function WorkspacesPage() {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Search and Pagination
  const [searchQuery, setSearchQuery] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Dialog States
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isRenameOpen, setIsRenameOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [activeWorkspace, setActiveWorkspace] = useState<Workspace | null>(null);

  // Forms
  const createForm = useForm<WorkspaceFormValues>({
    resolver: zodResolver(workspaceSchema),
    defaultValues: { name: "" },
  });

  const renameForm = useForm<WorkspaceFormValues>({
    resolver: zodResolver(workspaceSchema),
    defaultValues: { name: "" },
  });

  const fetchWorkspaces = async () => {
    try {
      setIsLoading(true);
      const response = await apiClient.get<Workspace[]>("/workspaces");
      setWorkspaces(response.data);
      setError(null);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      setError(e.response?.data?.detail || "Failed to load workspaces");
      toast.error("Failed to load workspaces");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchWorkspaces();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Derived State
  const filteredWorkspaces = useMemo(() => {
    return workspaces.filter((w) =>
      w.name.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }, [workspaces, searchQuery]);

  const totalPages = Math.ceil(filteredWorkspaces.length / ITEMS_PER_PAGE);
  
  const paginatedWorkspaces = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    return filteredWorkspaces.slice(start, start + ITEMS_PER_PAGE);
  }, [filteredWorkspaces, currentPage]);

  // Reset page when search changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery]);

  // Handlers
  const handleCreate = async (data: WorkspaceFormValues) => {
    try {
      await apiClient.post("/workspaces", { name: data.name });
      toast.success("Workspace created");
      setIsCreateOpen(false);
      createForm.reset();
      fetchWorkspaces();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to create workspace");
    }
  };

  const openRename = (workspace: Workspace) => {
    setActiveWorkspace(workspace);
    renameForm.setValue("name", workspace.name);
    setIsRenameOpen(true);
  };

  const handleRename = async (data: WorkspaceFormValues) => {
    if (!activeWorkspace) return;
    try {
      await apiClient.put(`/workspaces/${activeWorkspace.id}`, { name: data.name });
      toast.success("Workspace renamed");
      setIsRenameOpen(false);
      fetchWorkspaces();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to rename workspace");
    }
  };

  const openDelete = (workspace: Workspace) => {
    setActiveWorkspace(workspace);
    setIsDeleteOpen(true);
  };

  const handleDelete = async () => {
    if (!activeWorkspace) return;
    try {
      await apiClient.delete(`/workspaces/${activeWorkspace.id}`);
      toast.success("Workspace deleted");
      setIsDeleteOpen(false);
      
      if (paginatedWorkspaces.length === 1 && currentPage > 1) {
        setCurrentPage(prev => prev - 1);
      }
      
      fetchWorkspaces();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } } };
      toast.error(e.response?.data?.detail || "Failed to delete workspace");
    }
  };

  if (error) {
    return (
      <div className="flex h-[50vh] flex-col items-center justify-center gap-4 text-center">
        <h2 className="text-2xl font-bold tracking-tight">Failed to load workspaces</h2>
        <p className="text-muted-foreground">{error}</p>
        <Button onClick={fetchWorkspaces} variant="outline">Try again</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row gap-4 sm:items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Workspaces</h1>
          <p className="text-muted-foreground">Manage and collaborate across your workspaces.</p>
        </div>
        <Button onClick={() => setIsCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> New Workspace
        </Button>
      </div>

      <div className="flex items-center space-x-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            type="text"
            placeholder="Search workspaces..."
            className="pl-8"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardHeader className="gap-2">
                <Skeleton className="h-5 w-1/2" />
                <Skeleton className="h-4 w-4/5" />
              </CardHeader>
            </Card>
          ))}
        </div>
      ) : workspaces.length === 0 ? (
        <div className="flex h-[40vh] flex-col items-center justify-center gap-4 rounded-lg border border-dashed text-center">
          <Users className="h-10 w-10 text-muted-foreground" />
          <h2 className="text-xl font-semibold">No workspaces found</h2>
          <p className="max-w-sm text-muted-foreground">
            You don&apos;t belong to any workspaces yet. Create one to get started.
          </p>
          <Button onClick={() => setIsCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" /> Create Workspace
          </Button>
        </div>
      ) : filteredWorkspaces.length === 0 ? (
        <div className="flex h-[30vh] flex-col items-center justify-center text-center">
          <p className="text-muted-foreground">No workspaces match your search.</p>
        </div>
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {paginatedWorkspaces.map((workspace) => (
              <Card key={workspace.id} className="group relative hover:border-primary/50 transition-colors flex flex-col">
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div>
                      <CardTitle className="line-clamp-1 pr-6" title={workspace.name}>
                        <Link href={`/workspaces/${workspace.id}`} className="hover:underline">
                          {workspace.name}
                        </Link>
                      </CardTitle>
                      <CardDescription>
                        Created {new Date(workspace.createdAt).toLocaleDateString()}
                      </CardDescription>
                    </div>
                    
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="h-8 w-8 p-0 absolute top-4 right-4 opacity-0 group-hover:opacity-100 focus:opacity-100 transition-opacity">
                          <span className="sr-only">Open menu</span>
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => openRename(workspace)}>
                          <Pencil className="mr-2 h-4 w-4" /> Rename
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => openDelete(workspace)} className="text-destructive focus:text-destructive">
                          <Trash2 className="mr-2 h-4 w-4" /> Delete
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>

                  </div>
                </CardHeader>
                <div className="flex-1" />
                <CardFooter className="pt-4 pb-4">
                  <Button variant="secondary" className="w-full" asChild>
                    <Link href={`/workspaces/${workspace.id}`}>Open Workspace</Link>
                  </Button>
                </CardFooter>
              </Card>
            ))}
          </div>

          {totalPages > 1 && (
            <Pagination>
              <PaginationContent>
                <PaginationItem>
                  <Button 
                    variant="ghost" 
                    onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                  >
                    Previous
                  </Button>
                </PaginationItem>
                <PaginationItem className="flex items-center px-4 text-sm text-muted-foreground">
                  Page {currentPage} of {totalPages}
                </PaginationItem>
                <PaginationItem>
                  <Button 
                    variant="ghost" 
                    onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                  >
                    Next
                  </Button>
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          )}
        </>
      )}

      {/* Create Dialog */}
      <ResponsiveDialog 
        isOpen={isCreateOpen} 
        setIsOpen={setIsCreateOpen}
        title="Create Workspace"
        description="Add a new workspace to organize your boards."
      >
          <form onSubmit={createForm.handleSubmit(handleCreate)}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="create-name">Workspace Name</Label>
                <Input id="create-name" {...createForm.register("name")} placeholder="e.g. Project Alpha" />
                {createForm.formState.errors.name && (
                  <p className="text-sm text-destructive">{createForm.formState.errors.name.message}</p>
                )}
              </div>
            </div>
            <div className="flex flex-col-reverse sm:flex-row sm:justify-end gap-2 mt-4">
              <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={createForm.formState.isSubmitting}>
                {createForm.formState.isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create
              </Button>
            </div>
          </form>
      </ResponsiveDialog>

      {/* Rename Dialog */}
      <ResponsiveDialog 
        isOpen={isRenameOpen} 
        setIsOpen={setIsRenameOpen}
        title="Rename Workspace"
        description={`Change the name of ${activeWorkspace?.name}.`}
      >
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
            <div className="flex flex-col-reverse sm:flex-row sm:justify-end gap-2 mt-4">
              <Button type="button" variant="outline" onClick={() => setIsRenameOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={renameForm.formState.isSubmitting}>
                {renameForm.formState.isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save Changes
              </Button>
            </div>
          </form>
      </ResponsiveDialog>

      {/* Delete Alert Dialog */}
      <AlertDialog open={isDeleteOpen} onOpenChange={setIsDeleteOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the workspace &quot;{activeWorkspace?.name}&quot; and all of its boards. This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Delete Workspace
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

    </div>
  );
}
