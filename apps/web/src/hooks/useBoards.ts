import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { Board, BoardType } from '@/types/board';
import { toast } from 'sonner';

export interface UseBoardsResult {
  boards: Board[];
  isLoading: boolean;
  error: string | null;
  createBoard: (name: string, type: BoardType) => Promise<Board | null>;
  deleteBoard: (boardId: string) => Promise<boolean>;
  renameBoard: (boardId: string, name: string) => Promise<boolean>;
}

export function useBoards(workspaceId: string): UseBoardsResult {
  const [boards, setBoards] = useState<Board[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchBoards = useCallback(async (wId: string): Promise<void> => {
    if (!wId) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await apiClient.get<Board[]>(`/workspaces/${wId}/boards`);
      setBoards(response.data);
    } catch (err) {
      let errMsg = 'Failed to fetch boards';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      setError(errMsg);
      toast.error(errMsg);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBoards(workspaceId);
  }, [workspaceId, fetchBoards]);

  const createBoard = useCallback(async (name: string, type: BoardType): Promise<Board | null> => {
    try {
      const response = await apiClient.post<Board>(`/workspaces/${workspaceId}/boards`, { name, type });
      setBoards((prev) => [...prev, response.data]);
      toast.success('Board created successfully');
      return response.data;
    } catch (err) {
      let errMsg = 'Failed to create board';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return null;
    }
  }, [workspaceId]);

  const renameBoard = useCallback(async (boardId: string, name: string): Promise<boolean> => {
    try {
      const response = await apiClient.put<Board>(`/boards/${boardId}`, { name });
      setBoards((prev) =>
        prev.map((b) => (b.id === boardId ? response.data : b))
      );
      toast.success('Board renamed successfully');
      return true;
    } catch (err) {
      let errMsg = 'Failed to rename board';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, []);

  const deleteBoard = useCallback(async (boardId: string): Promise<boolean> => {
    const previousBoards = [...boards];
    // Optimistic delete
    setBoards((prev) => prev.filter((b) => b.id !== boardId));

    try {
      await apiClient.delete(`/boards/${boardId}`);
      toast.success('Board deleted successfully');
      return true;
    } catch (err) {
      // Revert on error
      setBoards(previousBoards);
      let errMsg = 'Failed to delete board';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, [boards]);

  return {
    boards,
    isLoading,
    error,
    createBoard,
    deleteBoard,
    renameBoard,
  };
}
