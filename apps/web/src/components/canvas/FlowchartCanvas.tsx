"use client";

import { useEffect, useCallback, useRef } from "react";
import {
  ReactFlow,
  MiniMap,
  Controls,
  Background,
  BackgroundVariant,
  Panel,
  Node,
  Viewport,
  useReactFlow,
  SelectionMode
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { useCanvasStore } from "@/store/useCanvasStore";
import { Button } from "@/components/ui/button";
import { PlusSquare, Undo, Redo, Loader2, CloudUpload, CloudOff, UserCircle2 } from "lucide-react";
import { RemoteCursors } from "./RemoteCursors";
import { RemoteSelectionHighlights } from "./RemoteSelectionHighlights";
import { VersionHistory } from "./VersionHistory";
import { AiSidePanel } from "./AiSidePanel";

interface FlowchartCanvasProps {
  boardId: string;
  workspaceId: string;
  accessToken: string;
  userName: string;
  userId: string;
  token?: string;
}

export function FlowchartCanvas({ boardId, workspaceId, accessToken, userName, userId, token }: FlowchartCanvasProps) {
  const wrapperRef = useRef<HTMLDivElement>(null);
  const { screenToFlowPosition } = useReactFlow();
  
  const {
    nodes,
    edges,
    onNodesChange,
    onEdgesChange,
    onConnect,
    initialize,
    loadViewportElements,
    addNode,
    syncStatus,
    undo,
    redo,
    commitHistory,
    past,
    future,
    saveNodePosition,
    deleteElements,
    processQueue,
    selectAll,
    updateCursor,
    remoteUsers,
    previewNodes,
    previewEdges,
    showCurrentInPreview
  } = useCanvasStore();

  const visibleNodes = showCurrentInPreview ? [...nodes, ...previewNodes] : previewNodes;
  const visibleEdges = showCurrentInPreview ? [...edges, ...previewEdges] : previewEdges;

  useEffect(() => {
    initialize(boardId, workspaceId, accessToken, userName, userId, "Flowchart", token);

    const handleOnline = () => {
      processQueue();
    };

    window.addEventListener('online', handleOnline);
    return () => window.removeEventListener('online', handleOnline);
  }, [boardId, workspaceId, accessToken, userName, userId, token, initialize, processQueue]);

  const onPaneMouseMove = useCallback((event: React.MouseEvent) => {
    const position = screenToFlowPosition({
      x: event.clientX,
      y: event.clientY,
    });
    updateCursor(position);
  }, [screenToFlowPosition, updateCursor]);

  const onPaneMouseLeave = useCallback(() => {
    updateCursor(null);
  }, [updateCursor]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const isCmdOrCtrl = e.ctrlKey || e.metaKey;
      
      if (isCmdOrCtrl && e.key === 'z') {
        if (e.shiftKey) {
          e.preventDefault();
          redo();
        } else {
          e.preventDefault();
          undo();
        }
        return;
      }

      if (isCmdOrCtrl && e.key === 'a') {
        e.preventDefault();
        selectAll();
        return;
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [undo, redo, selectAll]);

  const handleAddNode = () => {
    addNode("Rectangle", { x: Math.random() * 200, y: Math.random() * 200 });
  };

  const onNodeDragStart = useCallback(() => {
    commitHistory();
  }, [commitHistory]);

  const onNodeDragStop = useCallback((event: any, node: Node) => {
    saveNodePosition(node);
  }, [saveNodePosition]);

  const onNodesDelete = useCallback((deleted: any[]) => {
    deleteElements(deleted, []);
  }, [deleteElements]);

  const onEdgesDelete = useCallback((deleted: any[]) => {
    deleteElements([], deleted);
  }, [deleteElements]);

  const onMoveEnd = useCallback((event: any, viewport: Viewport) => {
    if (!wrapperRef.current) return;
    const width = wrapperRef.current.clientWidth;
    const height = wrapperRef.current.clientHeight;

    const canvasX = -viewport.x / viewport.zoom;
    const canvasY = -viewport.y / viewport.zoom;
    const canvasWidth = width / viewport.zoom;
    const canvasHeight = height / viewport.zoom;

    loadViewportElements(canvasX, canvasY, canvasWidth, canvasHeight);
  }, [loadViewportElements]);

  return (
    <div ref={wrapperRef} style={{ width: "100%", height: "100%" }}>
      <ReactFlow
        nodes={visibleNodes}
        edges={visibleEdges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onNodeDragStart={onNodeDragStart}
        onNodeDragStop={onNodeDragStop}
        onNodesDelete={onNodesDelete}
        onEdgesDelete={onEdgesDelete}
        onMoveEnd={onMoveEnd}
        onPaneMouseMove={onPaneMouseMove}
        onPaneMouseLeave={onPaneMouseLeave}
        panOnScroll
        selectionOnDrag
        panOnDrag={[1, 2]}
        selectionMode={SelectionMode.Partial}
        deleteKeyCode={["Backspace", "Delete"]}
        fitView
      >
        <Controls />
        <MiniMap />
        <Background variant={BackgroundVariant.Dots} gap={12} size={1} />
        
        <RemoteSelectionHighlights />
        <RemoteCursors />

        <AiSidePanel />

        <Panel position="top-left" className="bg-background/80 backdrop-blur p-2 rounded-lg border shadow-sm flex gap-2 items-center">
          <Button variant="secondary" size="sm" onClick={handleAddNode}>
            <PlusSquare className="w-4 h-4 mr-2" /> Add Node
          </Button>
          <div className="w-px h-6 bg-border mx-2" />
          <Button variant="ghost" size="icon" onClick={undo} disabled={past.length === 0}>
            <Undo className="w-4 h-4" />
          </Button>
          <Button variant="ghost" size="icon" onClick={redo} disabled={future.length === 0}>
            <Redo className="w-4 h-4" />
          </Button>
          
          <VersionHistory />

          <div className="text-xs text-muted-foreground ml-2 flex items-center gap-1 w-24">
            {syncStatus === 'saving' && <><Loader2 className="w-3 h-3 text-blue-500 animate-spin" /> Saving</>}
            {syncStatus === 'saved' && <><CloudUpload className="w-3 h-3 text-green-500" /> Saved</>}
            {syncStatus === 'failed' && <><CloudOff className="w-3 h-3 text-destructive" /> Offline</>}
            {syncStatus === 'idle' && <><CloudUpload className="w-3 h-3 text-muted-foreground" /> Synced</>}
          </div>

          <div className="w-px h-6 bg-border mx-2" />
          
          <div className="flex -space-x-2">
            {Object.values(remoteUsers).slice(0, 5).map((user, i) => (
              <div
                key={i}
                className="flex h-6 w-6 items-center justify-center rounded-full border-2 border-background text-[10px] font-bold text-white shadow-sm"
                style={{ backgroundColor: user.color }}
                title={user.name}
              >
                {user.name.charAt(0).toUpperCase()}
              </div>
            ))}
            {Object.values(remoteUsers).length > 5 && (
              <div className="flex h-6 w-6 items-center justify-center rounded-full border-2 border-background bg-muted text-[10px] font-medium text-muted-foreground shadow-sm">
                +{Object.values(remoteUsers).length - 5}
              </div>
            )}
            {Object.values(remoteUsers).length === 0 && (
              <div className="flex h-6 w-6 items-center justify-center rounded-full border border-dashed text-muted-foreground">
                <UserCircle2 className="h-4 w-4" />
              </div>
            )}
          </div>
        </Panel>
      </ReactFlow>
    </div>
  );
}
