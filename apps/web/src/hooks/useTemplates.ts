import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { BoardTemplateResponse } from '@/types/template';
import { toast } from 'sonner';

export interface UseTemplatesResult {
  templates: BoardTemplateResponse[];
  isLoading: boolean;
  applyTemplate: (boardId: string, templateId: string) => Promise<boolean>;
}

export function useTemplates(boardType?: string): UseTemplatesResult {
  const [templates, setTemplates] = useState<BoardTemplateResponse[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const fetchTemplates = useCallback(async (): Promise<void> => {
    setIsLoading(true);
    try {
      const url = boardType ? `/templates?boardType=${boardType}` : '/templates';
      const response = await apiClient.get<BoardTemplateResponse[]>(url);
      setTemplates(response.data);
    } catch (err) {
      let errMsg = 'Failed to load templates';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
    } finally {
      setIsLoading(false);
    }
  }, [boardType]);

  useEffect(() => {
    fetchTemplates();
  }, [fetchTemplates]);

  const applyTemplate = useCallback(async (boardId: string, templateId: string): Promise<boolean> => {
    try {
      await apiClient.post(`/boards/${boardId}/templates/${templateId}/apply`);
      toast.success('Template applied successfully');
      return true;
    } catch (err) {
      let errMsg = 'Failed to apply template';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
      return false;
    }
  }, []);

  return {
    templates,
    isLoading,
    applyTemplate,
  };
}
