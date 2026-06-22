import { useState, useCallback } from 'react';
import axios from 'axios';
import { apiClient } from '@/lib/api';
import { toast } from 'sonner';

export interface UseExportResult {
  isExporting: boolean;
  exportProgress: number;
  exportAs: (format: 'png' | 'jpg' | 'pdf' | 'svg' | 'flowspace' | 'drawio') => Promise<void>;
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

  const exportAs = useCallback(async (format: 'png' | 'jpg' | 'pdf' | 'svg' | 'flowspace' | 'drawio'): Promise<void> => {
    setIsExporting(true);
    setExportProgress(0);
    const toastId = toast.loading(`Preparing export as ${format.toUpperCase()}...`);

    try {
      const response = await apiClient.get(`/interop/boards/${boardId}/export`, {
        params: { format },
        responseType: 'blob'
      });
      
      const contentType = String(response.headers['content-type'] || '');
      
      if (contentType.includes('application/json')) {
        // Response is JSON blob, check if it contains jobId or direct file payload
        const text = await response.data.text();
        interface ExportJobData {
          jobId?: string;
        }
        let data: ExportJobData;
        try {
          data = JSON.parse(text) as ExportJobData;
        } catch (e) {
          data = {};
        }
        
        if (data.jobId) {
          const jobId = data.jobId;
          
          const poll = async (): Promise<void> => {
            try {
              const pollResponse = await apiClient.get(`/interop/boards/${boardId}/export/${jobId}`, {
                responseType: 'blob',
              });
              
              const pollContentType = String(pollResponse.headers['content-type'] || '');
              
              if (pollContentType.includes('application/json')) {
                const pollText = await pollResponse.data.text();
                interface JobStatusResponse {
                  status?: 'Pending' | 'Running' | 'Completed' | 'Failed';
                  progress?: number;
                  downloadUrl?: string;
                  error?: string;
                }
                const jobStatus = JSON.parse(pollText) as JobStatusResponse;
                
                if (jobStatus.progress !== undefined) {
                  setExportProgress(jobStatus.progress);
                }
                
                if (jobStatus.status === 'Completed') {
                  if (jobStatus.downloadUrl) {
                    const downloadRes = await axios.get(jobStatus.downloadUrl, { responseType: 'blob' });
                    triggerDownload(downloadRes.data, format, downloadRes.headers as Record<string, string>);
                    setExportProgress(100);
                    toast.success('Export complete!', { id: toastId });
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
                toast.success('Export complete!', { id: toastId });
                setIsExporting(false);
              }
            } catch (pollErr) {
              setIsExporting(false);
              let errMsg = 'Export failed during polling';
              if (axios.isAxiosError(pollErr)) {
                const errData = pollErr.response?.data as { detail?: string } | undefined;
                errMsg = errData?.detail || pollErr.message || errMsg;
              } else if (pollErr instanceof Error) {
                errMsg = pollErr.message;
              }
              toast.error(errMsg, { id: toastId });
            }
          };
          
          setTimeout(poll, 2000);
        } else {
          // If no jobId, it is direct serialized board content (like .flowspace)
          triggerDownload(response.data, format, response.headers as Record<string, string>);
          setExportProgress(100);
          toast.success('Export complete!', { id: toastId });
          setIsExporting(false);
        }
      } else {
        // Immediate download of the file blob (PNG, JPG, PDF, SVG)
        triggerDownload(response.data, format, response.headers as Record<string, string>);
        setExportProgress(100);
        toast.success('Export complete!', { id: toastId });
        setIsExporting(false);
      }
    } catch (err) {
      setIsExporting(false);
      let errMsg = 'Export failed';
      if (axios.isAxiosError(err)) {
        try {
          const dataBlob = err.response?.data as Blob | undefined;
          if (dataBlob) {
            const text = await dataBlob.text();
            const detail = (JSON.parse(text || '{}') as { detail?: string }).detail;
            errMsg = detail || err.message || errMsg;
          } else {
            errMsg = err.message || errMsg;
          }
        } catch {
          errMsg = err.message || errMsg;
        }
      } else if (err instanceof Error) {
        errMsg = err.message;
      }
      toast.error(errMsg, { id: toastId });
    }
  }, [boardId, triggerDownload]);

  return {
    isExporting,
    exportProgress,
    exportAs,
  };
}
