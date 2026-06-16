"use client";

import { useEffect, useState } from "react";
import { useCanvasStore } from "@/store/useCanvasStore";
import { BoardVersionResponse } from "@/types/version";
import { 
  History, Clock, User, ArrowLeftRight, RotateCcw, Save, Loader2 
} from "lucide-react";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import { 
  Sheet, 
  SheetContent, 
  SheetDescription, 
  SheetHeader, 
  SheetTitle, 
  SheetTrigger 
} from "@/components/ui/sheet";
import { ScrollArea } from "@/components/ui/scroll-area";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export function VersionHistory() {
  const { fetchVersions, saveVersion, restoreVersion } = useCanvasStore();
  const [versions, setVersions] = useState<BoardVersionResponse[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [saveName, setSaveName] = useState("");
  const [isSaveDialogOpen, setIsSaveDialogOpen] = useState(false);

  const loadVersions = async () => {
    setIsLoading(true);
    const data = await fetchVersions();
    setVersions(data);
    setIsLoading(false);
  };

  const handleSaveVersion = async () => {
    if (!saveName.trim()) return;
    setIsSaving(true);
    await saveVersion(saveName);
    setSaveName("");
    setIsSaveDialogOpen(false);
    setIsSaving(false);
    loadVersions();
  };

  const handleRestore = async (versionId: string) => {
    toast.promise(restoreVersion(versionId), {
      loading: 'Restoring version...',
      success: 'Version restored successfully',
      error: 'Failed to restore version'
    });
  };

  return (
    <Sheet onOpenChange={(open) => open && loadVersions()}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" title="Version History">
          <History className="h-4 w-4" />
        </Button>
      </SheetTrigger>
      <SheetContent className="w-[400px] sm:w-[540px]">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Version History
          </SheetTitle>
          <SheetDescription>
            View and restore previous snapshots of this board.
          </SheetDescription>
        </SheetHeader>

        <div className="mt-6 flex flex-col gap-4">
          <Dialog open={isSaveDialogOpen} onOpenChange={setIsSaveDialogOpen}>
            <DialogTrigger asChild>
              <Button className="w-full">
                <Save className="mr-2 h-4 w-4" /> Save Current Version
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Save Version</DialogTitle>
                <DialogDescription>
                  Give this snapshot a name to easily find it later.
                </DialogDescription>
              </DialogHeader>
              <div className="grid gap-4 py-4">
                <div className="grid gap-2">
                  <Label htmlFor="version-name">Version Name</Label>
                  <Input
                    id="version-name"
                    value={saveName}
                    onChange={(e) => setSaveName(e.target.value)}
                    placeholder="e.g. Before major refactor"
                  />
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setIsSaveDialogOpen(false)}>Cancel</Button>
                <Button onClick={handleSaveVersion} disabled={isSaving || !saveName.trim()}>
                  {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Save Snapshot
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>

          <ScrollArea className="h-[calc(100vh-250px)] pr-4">
            {isLoading ? (
              <div className="flex h-40 items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : versions.length === 0 ? (
              <div className="text-center py-10 text-muted-foreground">
                No versions saved yet.
              </div>
            ) : (
              <div className="space-y-4">
                {versions.map((version) => (
                  <div key={version.id} className="group relative flex flex-col gap-2 rounded-lg border p-4 hover:bg-accent/50 transition-colors">
                    <div className="flex items-start justify-between">
                      <div className="flex flex-col gap-1">
                        <span className="font-semibold text-sm">
                          {version.name || "Untitled Version"}
                        </span>
                        <div className="flex items-center gap-2 text-xs text-muted-foreground">
                          <User className="h-3 w-3" />
                          {version.creatorName}
                          <span>•</span>
                          {format(new Date(version.createdAt), "MMM d, HH:mm")}
                        </div>
                      </div>
                      <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button 
                          variant="outline" 
                          size="sm" 
                          className="h-8 text-xs"
                          onClick={() => handleRestore(version.id)}
                        >
                          <RotateCcw className="mr-1 h-3 w-3" /> Restore
                        </Button>
                      </div>
                    </div>
                    {version.description && (
                      <p className="text-xs text-muted-foreground line-clamp-2">
                        {version.description}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            )}
          </ScrollArea>
        </div>
      </SheetContent>
    </Sheet>
  );
}
