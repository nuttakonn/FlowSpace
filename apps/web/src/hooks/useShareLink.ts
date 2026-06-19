import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { BoardSharingInfoResponse, ShareLinkResponse } from '@/types/sharing';
import { toast } from 'sonner';

export interface UseShareLinkResult {
  shareInfo: BoardSharingInfoResponse | null;
  links: ShareLinkResponse[];
  isLoading: boolean;
  createLink: (permission: string, expiresInDays?: number) => Promise<ShareLinkResponse | null>;
  revokeLink: (linkId: string) => Promise<boolean>;
  updateVisibility: (visibility: number) => Promise<boolean>;
  copyLinkToClipboard: (url: string) => void;
}

export function useShareLink(boardId: string): UseShareLinkResult {
  const [shareInfo, setShareInfo] = useState<BoardSharingInfoResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const links = shareInfo?.shareLinks || [];

  const fetchSharingInfo = useCallback(async (bId: string): Promise<void> => {
    if (!bId) return;
    setIsLoading(true);
    try {
      const response = await apiClient.get<BoardSharingInfoResponse>(`/boards/${bId}/sharing`);
      setShareInfo(response.data);
    } catch (err) {
      let errMsg = 'Failed to load sharing info';
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
    fetchSharingInfo(boardId);
  }, [boardId, fetchSharingInfo]);

  const createLink = useCallback(async (permission: string, expiresInDays?: number): Promise<ShareLinkResponse | null> => {
    try {
      const response = await apiClient.post<ShareLinkResponse>(`/boards/${boardId}/share-links`, {
        permission,
        expiresInDays,
      });
      toast.success('Share link generated');
      await fetchSharingInfo(boardId);
      return response.data;
    } catch (err) {
      let errMsg = 'Failed to generate link';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return null;
    }
  }, [boardId, fetchSharingInfo]);

  const revokeLink = useCallback(async (linkId: string): Promise<boolean> => {
    try {
      await apiClient.delete(`/boards/${boardId}/share-links/${linkId}`);
      toast.success('Link revoked');
      await fetchSharingInfo(boardId);
      return true;
    } catch (err) {
      let errMsg = 'Failed to revoke link';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, [boardId, fetchSharingInfo]);

  const updateVisibility = useCallback(async (visibility: number): Promise<boolean> => {
    try {
      await apiClient.put(`/boards/${boardId}/visibility`, { visibility });
      toast.success('Visibility updated');
      await fetchSharingInfo(boardId);
      return true;
    } catch (err) {
      let errMsg = 'Failed to update visibility';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, [boardId, fetchSharingInfo]);

  const copyLinkToClipboard = useCallback((url: string): void => {
    navigator.clipboard.writeText(url)
      .then(() => {
        toast.success('Link copied!');
      })
      .catch(() => {
        toast.error('Failed to copy link');
      });
  }, []);

  return {
    shareInfo,
    links,
    isLoading,
    createLink,
    revokeLink,
    updateVisibility,
    copyLinkToClipboard,
  };
}
