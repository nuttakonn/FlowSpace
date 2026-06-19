"use client";

import { useEffect, useCallback, useRef, useState } from "react";
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
  SelectionMode,
  ReactFlowProvider
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { useCanvasStore } from "@/store/useCanvasStore";
import { Button } from "@/components/ui/button";
import { PlusSquare, Undo, Redo, Loader2, CloudUpload, CloudOff, UserCircle2, ChevronLeft, ChevronRight } from "lucide-react";
import { RemoteCursors } from "./RemoteCursors";
import { RemoteSelectionHighlights } from "./RemoteSelectionHighlights";
import { VersionHistory } from "./VersionHistory";
import { FloatingToolbar } from "./FloatingToolbar";
import { RectangleNode, DiamondNode, CircleNode, DatabaseNode, CloudNode, BrowserNode, StickyNoteNode, IconNode, TextNode, InfrastructureNode, ClientNode, MobileNode } from "./nodes/CustomNodes";

const nodeTypes = {
  rectangle: RectangleNode,
  diamond: DiamondNode,
  circle: CircleNode,
  database: DatabaseNode,
  cloud: CloudNode,
  browser: BrowserNode,
  stickynote: StickyNoteNode,
  icon: IconNode,
  text: TextNode,
  infrastructure: InfrastructureNode,
  client: ClientNode,
  mobile: MobileNode
};

interface FlowchartCanvasProps {
  boardId: string;
  workspaceId: string;
  accessToken: string;
  userName: string;
  userId: string;
  token?: string;
}

export function FlowchartCanvas(props: FlowchartCanvasProps) {
  return (
    <ReactFlowProvider>
      <FlowchartCanvasContent {...props} />
    </ReactFlowProvider>
  );
}

function FlowchartCanvasContent({ boardId, workspaceId, accessToken, userName, userId, token }: FlowchartCanvasProps) {
  const wrapperRef = useRef<HTMLDivElement>(null);
  const { screenToFlowPosition } = useReactFlow();
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  
  const nodes = useCanvasStore(s => s.nodes);
  const edges = useCanvasStore(s => s.edges);
  const onNodesChange = useCanvasStore(s => s.onNodesChange);
  const onEdgesChange = useCanvasStore(s => s.onEdgesChange);
  const onConnect = useCanvasStore(s => s.onConnect);
  const initialize = useCanvasStore(s => s.initialize);
  const loadViewportElements = useCanvasStore(s => s.loadViewportElements);
  const addNode = useCanvasStore(s => s.addNode);
  const syncStatus = useCanvasStore(s => s.syncStatus);
  const undo = useCanvasStore(s => s.undo);
  const redo = useCanvasStore(s => s.redo);
  const commitHistory = useCanvasStore(s => s.commitHistory);
  const past = useCanvasStore(s => s.past);
  const future = useCanvasStore(s => s.future);
  const saveNodePosition = useCanvasStore(s => s.saveNodePosition);
  const deleteElements = useCanvasStore(s => s.deleteElements);
  const processQueue = useCanvasStore(s => s.processQueue);
  const selectAll = useCanvasStore(s => s.selectAll);
  const updateCursor = useCanvasStore(s => s.updateCursor);
  const remoteUsers = useCanvasStore(s => s.remoteUsers);
  const previewNodes = useCanvasStore(s => s.previewNodes);
  const previewEdges = useCanvasStore(s => s.previewEdges);
  const showCurrentInPreview = useCanvasStore(s => s.showCurrentInPreview);

  const visibleNodes = showCurrentInPreview ? [...nodes, ...previewNodes] : previewNodes;
  const visibleEdges = showCurrentInPreview ? [...edges, ...previewEdges] : previewEdges;

  useEffect(() => {
    console.log("🚀 FlowSpace Engine v2.5 Initializing Board:", boardId);
    const templateId = typeof window !== 'undefined' ? new URLSearchParams(window.location.search).get('template') || undefined : undefined;
    initialize(boardId, workspaceId, accessToken, userName, userId, "Flowchart", token, templateId);

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

  const handleAddNode = (type: string, data?: any) => {
    addNode(type, { x: Math.random() * 200, y: Math.random() * 200 }, data);
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
    <div ref={wrapperRef} className="absolute inset-0 overflow-hidden bg-background">
      <ReactFlow
        nodes={visibleNodes}
        edges={visibleEdges}
        nodeTypes={nodeTypes}
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

        <Panel position="top-left" className="ml-4 mt-20 flex gap-2 items-start pointer-events-auto">
          {isSidebarOpen ? (
            <div className="flex gap-2 items-start">
              <FloatingToolbar onAddNode={handleAddNode} />
              <Button 
                variant="outline" 
                size="icon" 
                className="h-10 w-10 bg-background border shadow-md rounded-lg hover:bg-muted"
                onClick={() => setIsSidebarOpen(false)}
                title="Collapse Shapes Panel"
              >
                <ChevronLeft className="h-5 w-5" />
              </Button>
            </div>
          ) : (
            <Button 
              variant="outline" 
              size="icon" 
              className="h-10 w-10 bg-background border shadow-md rounded-lg hover:bg-muted"
              onClick={() => setIsSidebarOpen(true)}
              title="Expand Shapes Panel"
            >
              <ChevronRight className="h-5 w-5" />
            </Button>
          )}
        </Panel>

        <Panel position="top-center" className="mt-4 bg-background/80 backdrop-blur p-1 rounded-lg border shadow-sm flex gap-1 items-center">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={undo} disabled={past.length === 0}>
            <Undo className="w-4 h-4" />
          </Button>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={redo} disabled={future.length === 0}>
            <Redo className="w-4 h-4" />
          </Button>
          <div className="w-px h-4 bg-border mx-1" />
          <VersionHistory />
          <div className="w-px h-4 bg-border mx-1" />
          <div className="text-[10px] text-muted-foreground px-2 flex items-center gap-1 min-w-[70px]">
            {syncStatus === 'saving' && <><Loader2 className="w-3 h-3 text-blue-500 animate-spin" /> Saving</>}
            {syncStatus === 'saved' && <><CloudUpload className="w-3 h-3 text-green-500" /> Saved</>}
            {syncStatus === 'failed' && <><CloudOff className="w-3 h-3 text-destructive" /> Offline</>}
            {syncStatus === 'idle' && <><CloudUpload className="w-3 h-3 text-muted-foreground" /> Synced</>}
          </div>
        </Panel>

        <Panel position="top-right" className="mt-4 mr-4 flex -space-x-2">
            {Object.values(remoteUsers).slice(0, 5).map((user, i) => (
              <div
                key={i}
                className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-background text-[10px] font-bold text-white shadow-sm"
                style={{ backgroundColor: user?.color || '#ccc' }}
                title={user?.name || 'Guest'}
              >
                {(user?.name || 'Guest').charAt(0).toUpperCase()}
              </div>
            ))}
            {Object.values(remoteUsers).length > 5 && (
              <div className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-muted text-[10px] font-medium text-muted-foreground shadow-sm">
                +{Object.values(remoteUsers).length - 5}
              </div>
            )}
            {Object.values(remoteUsers).length === 0 && (
              <div className="flex h-8 w-8 items-center justify-center rounded-full border border-dashed text-muted-foreground bg-background/50">
                <UserCircle2 className="h-4 w-4" />
              </div>
            )}
        </Panel>
      </ReactFlow>
    </div>
  );
}
