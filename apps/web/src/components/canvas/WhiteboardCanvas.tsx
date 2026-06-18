"use client";

import { useEffect, useCallback, useMemo } from "react";
import { Tldraw, createTLStore, defaultShapeUtils, Editor, TLRecord, TLStoreEventInfo, InstancePresenceRecordType } from "tldraw";
import "tldraw/tldraw.css";
import { useCanvasStore } from "@/store/useCanvasStore";
import { WhiteboardResponse } from "@/types/canvas";
import { apiClient } from "@/lib/api";

interface WhiteboardCanvasProps {
  boardId: string;
  workspaceId: string;
  accessToken: string;
  userName: string;
  userId: string;
  token?: string;
}

export function WhiteboardCanvas({ boardId, workspaceId, accessToken, userName, userId, token }: WhiteboardCanvasProps) {
  const initialize = useCanvasStore(s => s.initialize);
  const disconnect = useCanvasStore(s => s.disconnect);
  const yWhiteboard = useCanvasStore(s => s.yWhiteboard);
  const updateWhiteboard = useCanvasStore(s => s.updateWhiteboard);
  
  const store = useMemo(() => createTLStore({ shapeUtils: defaultShapeUtils }), []);

  useEffect(() => {
    const init = async () => {
      initialize(boardId, workspaceId, accessToken, userName, userId, "Whiteboard", token);
      
      if (yWhiteboard.size === 0) {
        try {
          const response = await apiClient.get<WhiteboardResponse>(`/boards/${boardId}/whiteboard`);
          Object.entries(response.data.records).forEach(([id, record]) => {
            yWhiteboard.set(id, record);
          });
        } catch (error) {
          console.error("Failed to load initial whiteboard state", error);
        }
      }
    };
    
    init();
    return () => disconnect();
  }, [boardId, workspaceId, accessToken, userName, userId, token, initialize, disconnect]);

  // Sync Yjs -> Tldraw (Remote updates)
  useEffect(() => {
    const handleYjsUpdate = () => {
      const records: TLRecord[] = [];
      yWhiteboard.forEach((val) => {
        if (val) records.push(val as TLRecord);
      });
      
      store.mergeRemoteChanges(() => {
        store.put(records);
      });
    };

    yWhiteboard.observe(handleYjsUpdate);
    handleYjsUpdate();

    return () => yWhiteboard.unobserve(handleYjsUpdate);
  }, [yWhiteboard, store]);

  // Sync Tldraw -> Yjs (Local updates)
  useEffect(() => {
    const cleanup = store.listen((event: TLStoreEventInfo) => {
      if (event.source !== 'user') return;

      const changes: Record<string, any> = {};
      
      const isSharable = (id: string) => 
        id.startsWith('shape:') || 
        id.startsWith('asset:') || 
        id.startsWith('page:') || 
        id.startsWith('document:') ||
        id.startsWith('user_presence:');

      Object.values(event.changes.added).forEach(record => {
        if (isSharable(record.id)) changes[record.id] = record;
      });
      
      Object.values(event.changes.updated).forEach(([_, record]) => {
        if (isSharable(record.id)) changes[record.id] = record;
      });
      
      Object.values(event.changes.removed).forEach(record => {
        if (isSharable(record.id)) changes[record.id] = null;
      });

      if (Object.keys(changes).length > 0) {
        updateWhiteboard(changes);
      }
    });

    return () => cleanup();
  }, [store, updateWhiteboard]);

  const handleMount = useCallback((editor: Editor) => {
    editor.user.updateUserPreferences({ name: userName });
    editor.updateInstanceState({ screenBounds: editor.getViewportScreenBounds() });
  }, [userName]);

  return (
    <div className="fixed inset-0 top-14 overflow-hidden bg-background">
      <Tldraw 
        store={store} 
        onMount={handleMount}
        autoFocus
      />
    </div>
  );
}
