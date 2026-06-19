import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import * as Y from 'yjs';
import { apiClient } from '@/lib/api';
import { useCanvasStore } from '@/store/useCanvasStore';
import { BoardVersionResponse } from '@/types/version';
import { toast } from 'sonner';

export interface UseVersionHistoryResult {
  versions: BoardVersionResponse[];
  selectedVersion: BoardVersionResponse | null;
  isLoading: boolean;
  isSaving: boolean;
  isRestoring: boolean;
  saveVersion: (label?: string) => Promise<boolean>;
  restoreVersion: (versionId: string, onRestored?: () => void) => Promise<boolean>;
  selectVersion: (version: BoardVersionResponse) => void;
  clearSelection: () => void;
}

export function useVersionHistory(boardId: string): UseVersionHistoryResult {
  const [versions, setVersions] = useState<BoardVersionResponse[]>([]);
  const [selectedVersion, setSelectedVersion] = useState<BoardVersionResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [isRestoring, setIsRestoring] = useState<boolean>(false);

  const fetchVersions = useCallback(async (bId: string): Promise<void> => {
    if (!bId) return;
    setIsLoading(true);
    try {
      const response = await apiClient.get<BoardVersionResponse[]>(`/boards/${bId}/versions`);
      setVersions(response.data);
    } catch (err) {
      let errMsg = 'Failed to fetch versions';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchVersions(boardId);
  }, [boardId, fetchVersions]);

  const saveVersion = useCallback(async (label?: string): Promise<boolean> => {
    if (!boardId) return false;
    const canvasState = useCanvasStore.getState();
    const { nodes, edges, yDoc, boardType } = canvasState;

    setIsSaving(true);
    try {
      const req = {
        name: label || 'Untitled Version',
        description: '',
        nodesData: boardType === 'Whiteboard' ? '[]' : JSON.stringify(nodes),
        edgesData: boardType === 'Whiteboard' ? '[]' : JSON.stringify(edges),
        yjsState: Array.from(Y.encodeStateAsUpdate(yDoc))
      };

      await apiClient.post(`/boards/${boardId}/versions`, req);
      toast.success('Version saved');
      await fetchVersions(boardId);
      return true;
    } catch (err) {
      let errMsg = 'Failed to save version';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    } finally {
      setIsSaving(false);
    }
  }, [boardId, fetchVersions]);

  const restoreVersion = useCallback(async (versionId: string, onRestored?: () => void): Promise<boolean> => {
    if (!boardId) return false;
    setIsRestoring(true);
    try {
      await apiClient.post(`/boards/${boardId}/versions/${versionId}/restore`);
      toast.success('Version restored');
      await fetchVersions(boardId);
      if (onRestored) {
        onRestored();
      }
      return true;
    } catch (err) {
      let errMsg = 'Failed to restore version';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    } finally {
      setIsRestoring(false);
    }
  }, [boardId, fetchVersions]);

  const selectVersion = useCallback((version: BoardVersionResponse): void => {
    setSelectedVersion(version);
  }, []);

  const clearSelection = useCallback((): void => {
    setSelectedVersion(null);
  }, []);

  return {
    versions,
    selectedVersion,
    isLoading,
    isSaving,
    isRestoring,
    saveVersion,
    restoreVersion,
    selectVersion,
    clearSelection,
  };
}
