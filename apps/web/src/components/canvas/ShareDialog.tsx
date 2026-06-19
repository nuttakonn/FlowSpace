"use client";

import { useEffect, useState } from "react";
import { useCanvasStore } from "@/store/useCanvasStore";
import { 
  Share2, Globe, Lock, Users, Copy, Check, Trash2, 
  UserPlus, Shield, Clock, Loader2
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { 
  Dialog, 
  DialogContent, 
  DialogDescription, 
  DialogHeader, 
  DialogTitle, 
  DialogTrigger 
} from "@/components/ui/dialog";
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { apiClient } from "@/lib/api";
import { BoardSharingInfoResponse, ShareLinkResponse } from "@/types/sharing";
import { toast } from "sonner";

interface ShareDialogProps {
  boardId: string;
}

export function ShareDialog({ boardId }: ShareDialogProps) {
  const [sharingInfo, setSharingInfo] = useState<BoardSharingInfoResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const getDisplayUrl = (url: string) => {
    if (typeof window !== 'undefined') {
      try {
        const parsed = new URL(url);
        return `${window.location.origin}${parsed.pathname}${parsed.search}`;
      } catch {
        return url;
      }
    }
    return url;
  };

  const loadSharingInfo = async () => {
    setIsLoading(true);
    try {
      const res = await apiClient.get<BoardSharingInfoResponse>(`/boards/${boardId}/sharing`);
      setSharingInfo(res.data);
    } catch (error) {
      toast.error("Failed to load sharing info");
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdateVisibility = async (visibility: number) => {
    try {
      await apiClient.put(`/boards/${boardId}/sharing/visibility`, { visibility });
      toast.success("Visibility updated");
      loadSharingInfo();
    } catch (error) {
      toast.error("Failed to update visibility");
    }
  };

  const handleCreateLink = async (role: number) => {
    setIsGenerating(true);
    try {
      await apiClient.post(`/boards/${boardId}/sharing/links`, { role });
      toast.success("Share link generated");
      loadSharingInfo();
    } catch (error) {
      toast.error("Failed to generate link");
    } finally {
      setIsGenerating(false);
    }
  };

  const handleRevokeLink = async (linkId: string) => {
    try {
      await apiClient.delete(`/boards/${boardId}/sharing/links/${linkId}`);
      toast.success("Link revoked");
      loadSharingInfo();
    } catch (error) {
      toast.error("Failed to revoke link");
    }
  };

  const copyToClipboard = (url: string, id: string) => {
    navigator.clipboard.writeText(getDisplayUrl(url));
    setCopiedId(id);
    toast.success("Link copied to clipboard");
    setTimeout(() => setCopiedId(null), 2000);
  };

  return (
    <Dialog onOpenChange={(open) => open && loadSharingInfo()}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm" className="gap-2">
          <Share2 className="h-4 w-4" /> Share
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Share2 className="h-5 w-5 text-primary" />
            Share Board
          </DialogTitle>
          <DialogDescription>
            Manage who can view and edit this board.
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex h-60 items-center justify-center">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        ) : sharingInfo && (
          <div className="flex flex-col gap-6">
            {/* Visibility Section */}
            <div className="space-y-3">
              <label className="text-xs font-bold uppercase tracking-widest text-muted-foreground">General Access</label>
              <div className="flex items-center justify-between rounded-xl border p-4 bg-muted/20">
                <div className="flex items-center gap-3">
                  {sharingInfo.visibility === 1 && <Lock className="h-5 w-5 text-muted-foreground" />}
                  {sharingInfo.visibility === 2 && <Users className="h-5 w-5 text-blue-500" />}
                  {sharingInfo.visibility === 3 && <Globe className="h-5 w-5 text-green-500" />}
                  <div className="flex flex-col">
                    <span className="text-sm font-bold">
                      {sharingInfo.visibility === 1 && "Private"}
                      {sharingInfo.visibility === 2 && "Workspace Members"}
                      {sharingInfo.visibility === 3 && "Public (Link Required)"}
                    </span>
                    <span className="text-[10px] text-muted-foreground font-medium">
                      {sharingInfo.visibility === 1 && "Only people invited can access."}
                      {sharingInfo.visibility === 2 && "Everyone in this workspace can access."}
                      {sharingInfo.visibility === 3 && "Anyone with a link can access."}
                    </span>
                  </div>
                </div>
                <Select 
                  value={sharingInfo.visibility.toString()} 
                  onValueChange={(v) => handleUpdateVisibility(parseInt(v))}
                >
                  <SelectTrigger className="w-[140px] h-9">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="1">Private</SelectItem>
                    <SelectItem value="2">Workspace</SelectItem>
                    <SelectItem value="3">Public</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Public Links Section */}
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <label className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Public Share Links</label>
                <Button variant="ghost" size="sm" className="h-8 text-[10px] font-bold uppercase" onClick={() => handleCreateLink(4)} disabled={isGenerating}>
                  {isGenerating ? <Loader2 className="mr-1 h-3 w-3 animate-spin" /> : <UserPlus className="mr-1 h-3 w-3" />}
                  Generate Link
                </Button>
              </div>
              <ScrollArea className="max-h-[150px]">
                <div className="flex flex-col gap-2">
                  {sharingInfo.shareLinks.length === 0 ? (
                    <p className="text-center py-4 text-xs text-muted-foreground italic">No active share links.</p>
                  ) : sharingInfo.shareLinks.map((link) => (
                    <div key={link.id} className="flex items-center justify-between rounded-lg border p-3 bg-muted/5 group">
                      <div className="flex items-center gap-3">
                        <Badge variant="outline" className="text-[9px] font-black uppercase">
                          {link.role === 1 && "Owner"}
                          {link.role === 2 && "Editor"}
                          {link.role === 3 && "Commenter"}
                          {link.role === 4 && "Viewer"}
                        </Badge>
                        <div className="flex flex-col">
                          <a 
                            href={getDisplayUrl(link.url)} 
                            target="_blank" 
                            rel="noopener noreferrer" 
                            className="text-[10px] font-mono text-primary hover:underline truncate max-w-[200px]"
                          >
                            {getDisplayUrl(link.url)}
                          </a>
                          {link.expiresAt && (
                            <span className="text-[8px] text-amber-500 font-bold flex items-center gap-1">
                              <Clock className="h-2 w-2" /> Expires: {new Date(link.expiresAt).toLocaleDateString()}
                            </span>
                          )}
                        </div>
                      </div>
                      <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => copyToClipboard(link.url, link.id)}>
                          {copiedId === link.id ? <Check className="h-3.5 w-3.5 text-green-500" /> : <Copy className="h-3.5 w-3.5" />}
                        </Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleRevokeLink(link.id)}>
                          <Trash2 className="h-3.5 w-3.5" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </div>

            {/* People with Access */}
            <div className="space-y-3">
              <label className="text-xs font-bold uppercase tracking-widest text-muted-foreground">People with direct access</label>
              <div className="flex flex-col gap-3">
                {sharingInfo.permissions.map((p) => (
                  <div key={p.userId} className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-[10px] font-bold text-primary">
                        {(p.displayName || p.userEmail || "?").charAt(0).toUpperCase()}
                      </div>
                      <div className="flex flex-col">
                        <span className="text-sm font-bold">{p.displayName}</span>
                        <span className="text-[10px] text-muted-foreground">{p.userEmail}</span>
                      </div>
                    </div>
                    <Badge variant="secondary" className="text-[10px] font-bold uppercase tracking-tight">
                      {p.role === 1 && "Owner"}
                      {p.role === 2 && "Editor"}
                      {p.role === 4 && "Viewer"}
                    </Badge>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
