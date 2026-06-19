import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { Workspace, WorkspaceMember } from '@/types/workspace';
import { toast } from 'sonner';

export interface UseWorkspacesResult {
  workspaces: Workspace[];
  currentWorkspace: Workspace | null;
  members: WorkspaceMember[];
  isLoading: boolean;
  createWorkspace: (name: string) => Promise<Workspace | null>;
  selectWorkspace: (workspaceId: string) => void;
  inviteMember: (email: string, role: string) => Promise<boolean>;
  removeMember: (userId: string) => Promise<boolean>;
}

export function useWorkspaces(): UseWorkspacesResult {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [currentWorkspace, setCurrentWorkspace] = useState<Workspace | null>(null);
  const [members, setMembers] = useState<WorkspaceMember[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  // Fetch all workspaces
  const fetchWorkspaces = useCallback(async (): Promise<void> => {
    setIsLoading(true);
    try {
      const response = await apiClient.get<Workspace[]>('/workspaces');
      setWorkspaces(response.data);
      // Select the first workspace as default if none selected
      if (response.data.length > 0 && !currentWorkspace) {
        setCurrentWorkspace(response.data[0]);
      }
    } catch (err) {
      let errMsg = 'Failed to load workspaces';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
    } finally {
      setIsLoading(false);
    }
  }, [currentWorkspace]);

  // Fetch members for a specific workspace
  const fetchMembers = useCallback(async (workspaceId: string): Promise<void> => {
    try {
      const response = await apiClient.get<WorkspaceMember[]>(`/workspaces/${workspaceId}/members`);
      setMembers(response.data);
    } catch (err) {
      let errMsg = 'Failed to load workspace members';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
    }
  }, []);

  // Fetch workspaces on mount
  useEffect(() => {
    fetchWorkspaces();
  }, [fetchWorkspaces]);

  // Fetch members when currentWorkspace changes
  useEffect(() => {
    if (currentWorkspace) {
      fetchMembers(currentWorkspace.id);
    } else {
      setMembers([]);
    }
  }, [currentWorkspace, fetchMembers]);

  const createWorkspace = useCallback(async (name: string): Promise<Workspace | null> => {
    try {
      const response = await apiClient.post<Workspace>('/workspaces', { name });
      setWorkspaces((prev) => [...prev, response.data]);
      if (!currentWorkspace) {
        setCurrentWorkspace(response.data);
      }
      toast.success('Workspace created successfully');
      return response.data;
    } catch (err) {
      let errMsg = 'Failed to create workspace';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return null;
    }
  }, [currentWorkspace]);

  const selectWorkspace = useCallback((workspaceId: string): void => {
    const ws = workspaces.find((w) => w.id === workspaceId);
    if (ws) {
      setCurrentWorkspace(ws);
    }
  }, [workspaces]);

  const inviteMember = useCallback(async (email: string, role: string): Promise<boolean> => {
    if (!currentWorkspace) {
      toast.error('No active workspace selected');
      return false;
    }
    try {
      await apiClient.post(`/workspaces/${currentWorkspace.id}/members`, { email, role });
      toast.success(`Successfully invited ${email}`);
      // Refresh members list
      await fetchMembers(currentWorkspace.id);
      return true;
    } catch (err) {
      let errMsg = 'Failed to invite member';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, [currentWorkspace, fetchMembers]);

  const removeMember = useCallback(async (userId: string): Promise<boolean> => {
    if (!currentWorkspace) {
      toast.error('No active workspace selected');
      return false;
    }
    
    const previousMembers = [...members];
    // Optimistic delete of member
    setMembers((prev) => prev.filter((m) => m.userId !== userId));

    try {
      await apiClient.delete(`/workspaces/${currentWorkspace.id}/members/${userId}`);
      toast.success('Member removed successfully');
      return true;
    } catch (err) {
      // Revert on error
      setMembers(previousMembers);
      let errMsg = 'Failed to remove member';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, [currentWorkspace, members]);

  return {
    workspaces,
    currentWorkspace,
    members,
    isLoading,
    createWorkspace,
    selectWorkspace,
    inviteMember,
    removeMember,
  };
}
