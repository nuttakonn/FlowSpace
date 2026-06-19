import { useState, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { toast } from 'sonner';

export interface UseExportResult {
  isExporting: boolean;
  exportProgress: number;
  exportAs: (format: 'png' | 'pdf' | 'svg') => Promise<void>;
}

export function useExport(boardId: string): UseExportResult {
  const [isExporting, setIsExporting] = useState<boolean>(false);
  const [exportProgress, setExportProgress] = useState<number>(0);

  const triggerDownload = useCallback((blob: Blob, format: string, headers: Record<string, string>): void => {
    const contentDisposition = headers['content-disposition'];
    let fileName = `board-export-${Date.now()}.${format}`;
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename="?([^";\n]*)"?/);
      if (fileNameMatch && fileNameMatch[1]) {
        fileName = fileNameMatch[1];
      }
    }

    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }, []);

  const exportAs = useCallback(async (format: 'png' | 'pdf' | 'svg'): Promise<void> => {
    setIsExporting(true);
    setExportProgress(0);
    toast.info('Exporting...');

    try {
      const response = await apiClient.post(`/boards/${boardId}/export`, { format }, { responseType: 'blob' });
      
      const contentType = String(response.headers['content-type'] || '');
      
      if (contentType.includes('application/json')) {
        // Response is JSON blob containing jobId, read it as text
        const text = await response.data.text();
        const data = JSON.parse(text) as { jobId?: string };
        
        if (data.jobId) {
          const jobId = data.jobId;
          
          const poll = async (): Promise<void> => {
            try {
              const pollResponse = await apiClient.get(`/boards/${boardId}/export/${jobId}`, {
                responseType: 'blob',
              });
              
              const pollContentType = String(pollResponse.headers['content-type'] || '');
              
              if (pollContentType.includes('application/json')) {
                const pollText = await pollResponse.data.text();
                const jobStatus = JSON.parse(pollText) as {
                  status?: 'Pending' | 'Running' | 'Completed' | 'Failed';
                  progress?: number;
                  downloadUrl?: string;
                  error?: string;
                };
                
                if (jobStatus.progress !== undefined) {
                  setExportProgress(jobStatus.progress);
                }
                
                if (jobStatus.status === 'Completed') {
                  if (jobStatus.downloadUrl) {
                    const downloadRes = await axios.get(jobStatus.downloadUrl, { responseType: 'blob' });
                    triggerDownload(downloadRes.data, format, downloadRes.headers as Record<string, string>);
                    setExportProgress(100);
                    toast.success('Export complete!');
                    setIsExporting(false);
                    return;
                  }
                  throw new Error('Export job completed but download URL is missing.');
                } else if (jobStatus.status === 'Failed') {
                  throw new Error(jobStatus.error || 'Export job failed.');
                }
                
                // Poll again in 2 seconds
                setTimeout(poll, 2000);
              } else {
                // Done polling, got file blob directly
                triggerDownload(pollResponse.data, format, pollResponse.headers as Record<string, string>);
                setExportProgress(100);
                toast.success('Export complete!');
                setIsExporting(false);
              }
            } catch (pollErr) {
              setIsExporting(false);
              let errMsg = 'Export failed during polling';
              if (axios.isAxiosError(pollErr)) {
                errMsg = pollErr.response?.data?.detail || pollErr.message || errMsg;
              } else if (pollErr instanceof Error) {
                errMsg = pollErr.message;
              }
              toast.error(errMsg);
            }
          };
          
          setTimeout(poll, 2000);
        } else {
          throw new Error('Invalid export response format');
        }
      } else {
        // Immediate download
        triggerDownload(response.data, format, response.headers as Record<string, string>);
        setExportProgress(100);
        toast.success('Export complete!');
        setIsExporting(false);
      }
    } catch (err) {
      setIsExporting(false);
      let errMsg = 'Export failed';
      if (axios.isAxiosError(err)) {
        errMsg = err.response?.data?.detail || err.message || errMsg;
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg);
    }
  }, [boardId, triggerDownload]);

  return {
    isExporting,
    exportProgress,
    exportAs,
  };
}
