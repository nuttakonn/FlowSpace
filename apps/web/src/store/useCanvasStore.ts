import { create } from 'zustand';
import {
  Connection,
  Edge,
  EdgeChange,
  Node,
  NodeChange,
  addEdge,
  applyNodeChanges,
  applyEdgeChanges,
  MarkerType,
} from '@xyflow/react';
import { v4 as uuidv4 } from 'uuid';
import * as Y from 'yjs';
import * as signalR from '@microsoft/signalr';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { apiClient } from '@/lib/api';
import { DiagramResponse, CreateNodeRequest, UpdateNodeRequest, CreateEdgeRequest } from '@/types/canvas';
import { BoardVersionResponse } from '@/types/version';
import { AiGenerationHistoryResponse } from '@/types/ai';
import { BoardTemplateResponse } from '@/types/template';
import { toast } from 'sonner';
import debounce from 'lodash/debounce';

export type MutationType = 'CREATE_NODE' | 'UPDATE_NODE' | 'DELETE_NODE' | 'CREATE_EDGE' | 'DELETE_EDGE' | 'UPDATE_WHITEBOARD';

export interface Mutation {
  id: string;
  type: MutationType;
  payload: any;
  tempId?: string;
}

interface HistoryState {
  nodes: Node[];
  edges: Edge[];
}

export type SyncStatus = 'idle' | 'saving' | 'saved' | 'failed' | 'syncing';

export interface RemoteUser {
  id: string;
  name: string;
  color: string;
  cursor?: { x: number; y: number };
  selection: string[];
  lastActive: number;
}

interface CanvasState {
  nodes: Node[];
  edges: Edge[];
  boardId: string | null;
  workspaceId: string | null;
  boardType: string | null;
  isLoading: boolean;

  // Real-time System
  yDoc: Y.Doc;
  yNodes: Y.Map<any>;
  yEdges: Y.Map<any>;
  yWhiteboard: Y.Map<any>;
  hubConnection: signalR.HubConnection | null;
  remoteUsers: Record<number, RemoteUser>;

  // History System
  past: HistoryState[];
  future: HistoryState[];

  // Mutation Queue System
  mutationQueue: Mutation[];
  syncStatus: SyncStatus;
  tempToRealIdMap: Record<string, string>;

  // Clipboard
  clipboard: HistoryState | null;

  // AI System
  isGeneratingAi: boolean;
  aiStatus: string;
  previewNodes: Node[];
  previewEdges: Edge[];
  aiHistory: AiGenerationHistoryResponse[];
  templates: BoardTemplateResponse[];
  showCurrentInPreview: boolean;

  initialize: (boardId: string, workspaceId: string, accessToken: string, userName: string, userId: string, boardType: string, token?: string, templateId?: string, createdAt?: string) => void;
  disconnect: () => void;
  loadViewportElements: (x: number, y: number, width: number, height: number) => Promise<void>;
  
  commitHistory: () => void;
  undo: () => void;
  redo: () => void;

  onNodesChange: (changes: NodeChange[]) => void;
  onEdgesChange: (changes: EdgeChange[]) => void;
  onConnect: (connection: Connection) => void;
  
  addNode: (type: string, position: { x: number; y: number }, data?: any) => void;
  updateNodeLabel: (id: string, label: string) => void;
  updateNodeColor: (id: string, color: string) => void;
  saveNodePosition: (node: Node) => void;
  deleteElements: (nodesToDelete: Node[], edgesToDelete: Edge[]) => void;
  
  copySelection: (selectedNodes: Node[], selectedEdges: Edge[]) => void;
  pasteSelection: (position?: { x: number; y: number }) => void;
  selectAll: () => void;

  updateCursor: (position: { x: number; y: number } | null) => void;
  updateSelection: (selectedIds: string[]) => void;

  saveVersion: (name?: string, description?: string) => Promise<void>;
  restoreVersion: (versionId: string) => Promise<void>;
  fetchVersions: () => Promise<BoardVersionResponse[]>;

  generateAiDiagram: (prompt: string, type: string, templateId?: string) => Promise<void>;
  previewAiDiagram: (prompt: string, type: string, templateId?: string, refinementCommand?: string) => Promise<void>;
  acceptAiPreview: () => void;
  rejectAiPreview: () => void;
  togglePreviewComparison: () => void;
  fetchAiHistory: () => Promise<void>;
  fetchTemplates: () => Promise<void>;

  // Whiteboard Actions
  updateWhiteboard: (records: any) => void;

  enqueueMutation: (mutation: Omit<Mutation, 'id'>) => void;
  processQueue: () => Promise<void>;
  resolveRealId: (id: string) => string;
}

const USER_COLORS = ['#ef4444', '#f97316', '#f59e0b', '#10b981', '#06b6d4', '#3b82f6', '#6366f1', '#8b5cf6', '#d946ef', '#f43f5e'];

export const useCanvasStore = create<CanvasState>((set, get) => {
  const debouncedProcessQueue = debounce(() => { get().processQueue(); }, 500);

  const yDoc = new Y.Doc();
  return {
    nodes: [], edges: [], boardId: null, workspaceId: null, boardType: null, isLoading: false, past: [], future: [], mutationQueue: [], syncStatus: 'idle',
    tempToRealIdMap: {}, clipboard: null, yDoc, yNodes: yDoc.getMap('nodes'), yEdges: yDoc.getMap('edges'), yWhiteboard: yDoc.getMap('whiteboard'),
    hubConnection: null, remoteUsers: {}, isGeneratingAi: false, aiStatus: '',
    previewNodes: [], previewEdges: [], aiHistory: [], templates: [], showCurrentInPreview: true,

    initialize: (boardId, workspaceId, accessToken, userName, userId, boardType, token, templateId, createdAt) => {
      const state = get();
      if (state.boardId === boardId && state.hubConnection?.state === signalR.HubConnectionState.Connected) {
        return;
      }

      // Important: clear the local state to prevent ghost nodes from the previous board
      const localYDoc = new Y.Doc();
      const localYNodes = localYDoc.getMap(`nodes_${boardId}`);
      const localYEdges = localYDoc.getMap(`edges_${boardId}`);
      const localYWhiteboard = localYDoc.getMap(`whiteboard_${boardId}`);

      set({ 
        boardId, 
        workspaceId, 
        boardType, 
        nodes: [], 
        edges: [], 
        past: [], 
        future: [], 
        mutationQueue: [], 
        syncStatus: 'idle', 
        tempToRealIdMap: {}, 
        clipboard: null, 
        remoteUsers: {},
        yDoc: localYDoc,
        yNodes: localYNodes,
        yEdges: localYEdges,
        yWhiteboard: localYWhiteboard
      });

      // Cleanup existing connection if any
      if (state.hubConnection) {
        state.hubConnection.stop();
      }
      
      const hubUrl = process.env.NEXT_PUBLIC_HUB_URL || 'https://flowspace-api-1kor.onrender.com/hubs/collaboration';
      const options: signalR.IHttpConnectionOptions = {};
      
      if (accessToken) {
        options.accessTokenFactory = () => accessToken;
      }

      const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, options)
        .withHubProtocol(new MessagePackHubProtocol())
        .withAutomaticReconnect()
        .build();
        
      const userColor = USER_COLORS[Math.floor(Math.random() * USER_COLORS.length)];

      set({ hubConnection: connection });

      localYDoc.on('update', (update) => { 
        if (connection.state === signalR.HubConnectionState.Connected) { 
          connection.invoke('UpdateCanvas', boardId, update); 
        } 
      });

      connection.on('OnUpdate', (update: Uint8Array) => { 
        Y.applyUpdate(localYDoc, update, 'remote'); 
      });

      connection.on('OnAwareness', (awarenessUpdate: Uint8Array) => {
        const state = JSON.parse(new TextDecoder().decode(awarenessUpdate));
        set((s) => ({ remoteUsers: { ...s.remoteUsers, [state.clientId]: state.userState } }));
      });
      
      localYNodes.observe((event) => { 
        const currentNodes = get().nodes;
        const deletedKeys = new Set<string>();
        event.keys.forEach((change, key) => {
          if (change.action === 'delete') {
            deletedKeys.add(key);
          }
        });

        const yNodeValues = Array.from(localYNodes.values()).map(n => {
          const node = n as Node;
          const localNode = currentNodes.find(cn => cn.id === node.id);
          return {
            ...node,
            selected: localNode ? localNode.selected : false
          };
        });

        const yNodeIds = new Set(yNodeValues.map(n => n.id));

        const remainingFetchedNodes = currentNodes.filter(n => {
          if (yNodeIds.has(n.id)) return false;
          if (deletedKeys.has(n.id)) return false;
          return true;
        });

        set({ nodes: [...yNodeValues, ...remainingFetchedNodes] }); 
      });
      
      localYEdges.observe((event) => { 
        const currentEdges = get().edges;
        const deletedKeys = new Set<string>();
        event.keys.forEach((change, key) => {
          if (change.action === 'delete') {
            deletedKeys.add(key);
          }
        });

        const yEdgeValues = Array.from(localYEdges.values()).map(e => {
          const edge = e as Edge;
          const localEdge = currentEdges.find(ce => ce.id === edge.id);
          return {
            ...edge,
            selected: localEdge ? localEdge.selected : false
          };
        });

        const yEdgeIds = new Set(yEdgeValues.map(e => e.id));

        const remainingFetchedEdges = currentEdges.filter(e => {
          if (yEdgeIds.has(e.id)) return false;
          if (deletedKeys.has(e.id)) return false;
          return true;
        });

        set({ edges: [...yEdgeValues, ...remainingFetchedEdges] }); 
      });

      connection.start().then(() => {
        connection.invoke('JoinBoard', boardId, token || null);
        connection.invoke('SendAwareness', boardId, new TextEncoder().encode(JSON.stringify({ 
          clientId: connection.connectionId, 
          userState: { id: userId, name: userName, color: userColor, selection: [], lastActive: Date.now() } 
        })));

        // Seeding templates if the board is brand new and empty
        setTimeout(async () => {
          let dbHasElements = false;
          try {
            const dbElements = await apiClient.get<DiagramResponse>(`/boards/${boardId}/elements`);
            dbHasElements = dbElements.data.nodes.length > 0 || dbElements.data.edges.length > 0;
          } catch (e) {
            console.error("Failed to check if board is empty", e);
          }

          const isNewlyCreated = createdAt ? (new Date().getTime() - new Date(createdAt).getTime() < 30000) : false;

          if (localYNodes.size === 0 && !dbHasElements && isNewlyCreated && templateId && templateId !== 'blank-flowchart') {
            console.log("Seeding template:", templateId);
            if (templateId === 'simple-flowchart') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'circle', position: { x: 250, y: 100 }, data: { label: 'Start' } };
              const n2: Node = { id: id2, type: 'rectangle', position: { x: 200, y: 250 }, data: { label: 'Process Step' } };
              const n3: Node = { id: id3, type: 'circle', position: { x: 250, y: 420 }, data: { label: 'End' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Circle', x: 250, y: 100, metadata: JSON.stringify({ label: 'Start' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Rectangle', x: 200, y: 250, metadata: JSON.stringify({ label: 'Process Step' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Circle', x: 250, y: 420, metadata: JSON.stringify({ label: 'End' }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id2, target: id3, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id2, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
            } else if (templateId === 'system-architecture') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              const id4 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'client', position: { x: 100, y: 200 }, data: { label: 'User Client' } };
              const n2: Node = { id: id2, type: 'infrastructure', position: { x: 300, y: 200 }, data: { label: 'Load Balancer', iconName: 'Network', color: 'bg-indigo-500/10 text-indigo-600', sublabel: 'Network' } };
              const n3: Node = { id: id3, type: 'infrastructure', position: { x: 550, y: 200 }, data: { label: 'API Gateway', iconName: 'Zap', color: 'bg-yellow-500/10 text-yellow-600', sublabel: 'API Gateway' } };
              const n4: Node = { id: id4, type: 'database', position: { x: 800, y: 200 }, data: { label: 'Production DB' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              localYNodes.set(id4, n4);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Client', x: 100, y: 200, metadata: JSON.stringify({ label: 'User Client' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Infrastructure', x: 300, y: 200, metadata: JSON.stringify({ label: 'Load Balancer', iconName: 'Network', color: 'bg-indigo-500/10 text-indigo-600', sublabel: 'Network' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Infrastructure', x: 550, y: 200, metadata: JSON.stringify({ label: 'API Gateway', iconName: 'Zap', color: 'bg-yellow-500/10 text-yellow-600', sublabel: 'API Gateway' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id4, payload: { type: 'Database', x: 800, y: 200, metadata: JSON.stringify({ label: 'Production DB' }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e3Id = `temp-${uuidv4()}`;
              
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id2, target: id3, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e3: Edge = { id: e3Id, source: id3, target: id4, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              localYEdges.set(e3Id, e3);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id2, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e3Id, payload: { sourceNodeId: id3, targetNodeId: id4, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
            } else if (templateId === 'brainstorming') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'stickynote', position: { x: 100, y: 100 }, data: { label: 'Idea 1: Core Features\n- Visual Canvas\n- Multi-user' } };
              const n2: Node = { id: id2, type: 'stickynote', position: { x: 320, y: 100 }, data: { label: 'Idea 2: Marketing\n- Product Hunt\n- Tech Blogs' } };
              const n3: Node = { id: id3, type: 'stickynote', position: { x: 100, y: 320 }, data: { label: 'Idea 3: Design\n- Clean aesthetics\n- Modern Accordion' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'StickyNote', x: 100, y: 100, metadata: JSON.stringify({ label: 'Idea 1: Core Features\n- Visual Canvas\n- Multi-user' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'StickyNote', x: 320, y: 100, metadata: JSON.stringify({ label: 'Idea 2: Marketing\n- Product Hunt\n- Tech Blogs' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'StickyNote', x: 100, y: 320, metadata: JSON.stringify({ label: 'Idea 3: Design\n- Clean aesthetics\n- Modern Accordion' }) } });
            } else if (templateId === 'org-chart') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'rectangle', position: { x: 300, y: 50 }, data: { label: 'CEO / Founder\n(Strategic Direction)' } };
              const n2: Node = { id: id2, type: 'rectangle', position: { x: 150, y: 200 }, data: { label: 'VP of Engineering\n(Technical Execution)' } };
              const n3: Node = { id: id3, type: 'rectangle', position: { x: 450, y: 200 }, data: { label: 'VP of Product\n(Product Roadmap)' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Rectangle', x: 300, y: 50, metadata: JSON.stringify({ label: 'CEO / Founder\n(Strategic Direction)' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Rectangle', x: 150, y: 200, metadata: JSON.stringify({ label: 'VP of Engineering\n(Technical Execution)' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Rectangle', x: 450, y: 200, metadata: JSON.stringify({ label: 'VP of Product\n(Product Roadmap)' }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id1, target: id3, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id1, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
            } else if (templateId === 'uml-class-diagram') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'rectangle', position: { x: 150, y: 80 }, data: { label: 'class User\n---\n- id: Guid\n- email: string\n---\n+ Register()\n+ Login()', width: 220, height: 140 } };
              const n2: Node = { id: id2, type: 'rectangle', position: { x: 480, y: 80 }, data: { label: 'class Account\n---\n- userId: Guid\n- isActive: bool\n---\n+ Activate()\n+ Suspend()', width: 220, height: 140 } };
              const n3: Node = { id: id3, type: 'rectangle', position: { x: 150, y: 300 }, data: { label: 'class Profile\n---\n- name: string\n- avatarUrl: string\n---\n+ UpdateInfo()', width: 220, height: 140 } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Rectangle', x: 150, y: 80, metadata: JSON.stringify({ label: 'class User\n---\n- id: Guid\n- email: string\n---\n+ Register()\n+ Login()', width: 220, height: 140 }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Rectangle', x: 480, y: 80, metadata: JSON.stringify({ label: 'class Account\n---\n- userId: Guid\n- isActive: bool\n---\n+ Activate()\n+ Suspend()', width: 220, height: 140 }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Rectangle', x: 150, y: 300, metadata: JSON.stringify({ label: 'class Profile\n---\n- name: string\n- avatarUrl: string\n---\n+ UpdateInfo()', width: 220, height: 140 }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id1, target: id3, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id1, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
            } else if (templateId === 'kanban-board') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              const id4 = `temp-${uuidv4()}`;
              const id5 = `temp-${uuidv4()}`;
              const id6 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'rectangle', position: { x: 100, y: 50 }, data: { label: 'To Do', width: 200, height: 50, color: 'bg-slate-100 text-slate-800 font-bold border-slate-300' } };
              const n2: Node = { id: id2, type: 'rectangle', position: { x: 350, y: 50 }, data: { label: 'In Progress', width: 200, height: 50, color: 'bg-blue-100 text-blue-800 font-bold border-blue-300' } };
              const n3: Node = { id: id3, type: 'rectangle', position: { x: 600, y: 50 }, data: { label: 'Done', width: 200, height: 50, color: 'bg-green-100 text-green-800 font-bold border-green-300' } };
              
              const n4: Node = { id: id4, type: 'stickynote', position: { x: 100, y: 120 }, data: { label: 'Design landing page mockup', width: 200, height: 100, color: 'bg-yellow-50/90' } };
              const n5: Node = { id: id5, type: 'stickynote', position: { x: 350, y: 120 }, data: { label: 'Develop workspace settings UI', width: 200, height: 100, color: 'bg-yellow-50/90' } };
              const n6: Node = { id: id6, type: 'stickynote', position: { x: 600, y: 120 }, data: { label: 'Initialize GitHub repository', width: 200, height: 100, color: 'bg-yellow-50/90' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              localYNodes.set(id4, n4);
              localYNodes.set(id5, n5);
              localYNodes.set(id6, n6);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Rectangle', x: 100, y: 50, metadata: JSON.stringify({ label: 'To Do', width: 200, height: 50, color: 'bg-slate-100 text-slate-800 font-bold border-slate-300' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Rectangle', x: 350, y: 50, metadata: JSON.stringify({ label: 'In Progress', width: 200, height: 50, color: 'bg-blue-100 text-blue-800 font-bold border-blue-300' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Rectangle', x: 600, y: 50, metadata: JSON.stringify({ label: 'Done', width: 200, height: 50, color: 'bg-green-100 text-green-800 font-bold border-green-300' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id4, payload: { type: 'StickyNote', x: 100, y: 120, metadata: JSON.stringify({ label: 'Design landing page mockup', width: 200, height: 100, color: 'bg-yellow-50/90' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id5, payload: { type: 'StickyNote', x: 350, y: 120, metadata: JSON.stringify({ label: 'Develop workspace settings UI', width: 200, height: 100, color: 'bg-yellow-50/90' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id6, payload: { type: 'StickyNote', x: 600, y: 120, metadata: JSON.stringify({ label: 'Initialize GitHub repository', width: 200, height: 100, color: 'bg-yellow-50/90' }) } });
            } else if (templateId === 'user-journey-map') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'circle', position: { x: 100, y: 150 }, data: { label: '1. Discovery\n(User discovers FlowSpace through Product Hunt)', width: 160, height: 160, color: 'border-yellow-400 bg-yellow-50/10' } };
              const n2: Node = { id: id2, type: 'circle', position: { x: 350, y: 150 }, data: { label: '2. Register\n(Fills onboarding details and invites colleagues)', width: 160, height: 160, color: 'border-blue-400 bg-blue-50/10' } };
              const n3: Node = { id: id3, type: 'circle', position: { x: 600, y: 150 }, data: { label: '3. Create Board\n(Starts collaborating in real-time on canvas)', width: 160, height: 160, color: 'border-green-400 bg-green-50/10' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Circle', x: 100, y: 150, metadata: JSON.stringify({ label: '1. Discovery\n(User discovers FlowSpace through Product Hunt)', width: 160, height: 160, color: 'border-yellow-400 bg-yellow-50/10' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Circle', x: 350, y: 150, metadata: JSON.stringify({ label: '2. Register\n(Fills onboarding details and invites colleagues)', width: 160, height: 160, color: 'border-blue-400 bg-blue-50/10' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Circle', x: 600, y: 150, metadata: JSON.stringify({ label: '3. Create Board\n(Starts collaborating in real-time on canvas)', width: 160, height: 160, color: 'border-green-400 bg-green-50/10' }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#eab308' } };
              const e2: Edge = { id: e2Id, source: id2, target: id3, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id2, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
            } else if (templateId === 'concept-mindmap') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              const id4 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'circle', position: { x: 350, y: 200 }, data: { label: 'FlowSpace Core', width: 180, height: 180, color: 'border-primary border-4 bg-primary/5 text-lg font-bold' } };
              const n2: Node = { id: id2, type: 'circle', position: { x: 100, y: 80 }, data: { label: 'Real-time Canvas\n(Yjs / WebSockets)', width: 140, height: 140, color: 'border-yellow-400 bg-yellow-50/10' } };
              const n3: Node = { id: id3, type: 'circle', position: { x: 600, y: 80 }, data: { label: 'Backend API\n(ASP.NET Core 10 / CQRS)', width: 140, height: 140, color: 'border-blue-400 bg-blue-50/10' } };
              const n4: Node = { id: id4, type: 'circle', position: { x: 370, y: 430 }, data: { label: 'Collaborative UI\n(Next.js / Tailwind)', width: 140, height: 140, color: 'border-green-400 bg-green-50/10' } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              localYNodes.set(id4, n4);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Circle', x: 350, y: 200, metadata: JSON.stringify({ label: 'FlowSpace Core', width: 180, height: 180, color: 'border-primary border-4 bg-primary/5 text-lg font-bold' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Circle', x: 100, y: 80, metadata: JSON.stringify({ label: 'Real-time Canvas\n(Yjs / WebSockets)', width: 140, height: 140, color: 'border-yellow-400 bg-yellow-50/10' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Circle', x: 600, y: 80, metadata: JSON.stringify({ label: 'Backend API\n(ASP.NET Core 10 / CQRS)', width: 140, height: 140, color: 'border-blue-400 bg-blue-50/10' }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id4, payload: { type: 'Circle', x: 370, y: 430, metadata: JSON.stringify({ label: 'Collaborative UI\n(Next.js / Tailwind)', width: 140, height: 140, color: 'border-green-400 bg-green-50/10' }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e3Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-left', targetHandle: 't-bottom', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id1, target: id3, sourceHandle: 's-right', targetHandle: 't-bottom', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e3: Edge = { id: e3Id, source: id1, target: id4, sourceHandle: 's-bottom', targetHandle: 't-top', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              localYEdges.set(e3Id, e3);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-left', targetHandle: 't-bottom' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id1, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-bottom' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e3Id, payload: { sourceNodeId: id1, targetNodeId: id4, metadata: JSON.stringify({ sourceHandle: 's-bottom', targetHandle: 't-top' }) } });
            } else if (templateId === 'project-timeline') {
              const id1 = `temp-${uuidv4()}`;
              const id2 = `temp-${uuidv4()}`;
              const id3 = `temp-${uuidv4()}`;
              
              const n1: Node = { id: id1, type: 'rectangle', position: { x: 100, y: 150 }, data: { label: 'Phase 1: Planning\n- Define scope\n- Wireframe design', width: 200, height: 100 } };
              const n2: Node = { id: id2, type: 'rectangle', position: { x: 380, y: 150 }, data: { label: 'Phase 2: Development\n- Build API services\n- Real-time sync setup', width: 200, height: 100 } };
              const n3: Node = { id: id3, type: 'rectangle', position: { x: 660, y: 150 }, data: { label: 'Phase 3: Launch\n- Integration tests\n- Production deploy', width: 200, height: 100 } };
              
              localYNodes.set(id1, n1);
              localYNodes.set(id2, n2);
              localYNodes.set(id3, n3);
              
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id1, payload: { type: 'Rectangle', x: 100, y: 150, metadata: JSON.stringify({ label: 'Phase 1: Planning\n- Define scope\n- Wireframe design', width: 200, height: 100 }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id2, payload: { type: 'Rectangle', x: 380, y: 150, metadata: JSON.stringify({ label: 'Phase 2: Development\n- Build API services\n- Real-time sync setup', width: 200, height: 100 }) } });
              get().enqueueMutation({ type: 'CREATE_NODE', tempId: id3, payload: { type: 'Rectangle', x: 660, y: 150, metadata: JSON.stringify({ label: 'Phase 3: Launch\n- Integration tests\n- Production deploy', width: 200, height: 100 }) } });
              
              const e1Id = `temp-${uuidv4()}`;
              const e2Id = `temp-${uuidv4()}`;
              const e1: Edge = { id: e1Id, source: id1, target: id2, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              const e2: Edge = { id: e2Id, source: id2, target: id3, sourceHandle: 's-right', targetHandle: 't-left', markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' } };
              
              localYEdges.set(e1Id, e1);
              localYEdges.set(e2Id, e2);
              
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e1Id, payload: { sourceNodeId: id1, targetNodeId: id2, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
              get().enqueueMutation({ type: 'CREATE_EDGE', tempId: e2Id, payload: { sourceNodeId: id2, targetNodeId: id3, metadata: JSON.stringify({ sourceHandle: 's-right', targetHandle: 't-left' }) } });
            }
          }
        }, 1000);

      }).catch((err) => {
        console.error('SignalR Connection Error:', err);
        toast.error('Failed to connect to real-time engine');
      });
    },

    updateCursor: (position) => {
      const { hubConnection, boardId } = get();
      if (!hubConnection || !boardId || hubConnection.state !== signalR.HubConnectionState.Connected) return;
      hubConnection.invoke('SendAwareness', boardId, new TextEncoder().encode(JSON.stringify({ clientId: hubConnection.connectionId, userState: { cursor: position, lastActive: Date.now() } })));
    },

    updateSelection: (selectedIds) => {
      const { hubConnection, boardId } = get();
      if (!hubConnection || !boardId || hubConnection.state !== signalR.HubConnectionState.Connected) return;
      hubConnection.invoke('SendAwareness', boardId, new TextEncoder().encode(JSON.stringify({ clientId: hubConnection.connectionId, userState: { selection: selectedIds, lastActive: Date.now() } })));
    },

    disconnect: () => {
      const { hubConnection } = get();
      if (hubConnection) { hubConnection.stop(); set({ hubConnection: null }); }
    },

    loadViewportElements: async (x, y, width, height) => {
      const state = get();
      if (!state.boardId || state.boardType === 'Whiteboard') return;
      const bufferX = width * 0.2, bufferY = height * 0.2;
      try {
        set({ isLoading: true });
        const response = await apiClient.get<DiagramResponse>(`/boards/${state.boardId}/elements`, { params: { x: x - bufferX, y: y - bufferY, width: width + bufferX * 2, height: height + bufferY * 2 } });
        
        const fetchedNodes = response.data.nodes.map(n => {
          let meta: any = {};
          try { if (n.metadata) meta = JSON.parse(n.metadata); } catch(err) {}
          return { id: n.id, type: n.type.toLowerCase(), position: { x: n.x, y: n.y }, data: { label: meta.label || n.type, ...meta } };
        });

        const fetchedEdges = response.data.edges.map(e => {
          let meta: any = {};
          try { if (e.metadata) meta = JSON.parse(e.metadata); } catch(err) {}
          return { 
            id: e.id, 
            source: e.sourceNodeId, 
            target: e.targetNodeId,
            sourceHandle: meta.sourceHandle || undefined,
            targetHandle: meta.targetHandle || undefined,
            markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' }
          };
        });

        set((s) => ({ 
          nodes: [...fetchedNodes, ...s.nodes.filter(n => n.id.startsWith('temp-'))], 
          edges: [...fetchedEdges, ...s.edges.filter(e => e.id.startsWith('temp-'))] 
        }));
      } catch (error) { toast.error('Failed to load viewport data'); } finally { set({ isLoading: false }); }
    },

    commitHistory: () => set((state) => ({ past: [...state.past, { nodes: state.nodes, edges: state.edges }].slice(-100), future: [] })),

    undo: () => set((state) => {
      if (state.past.length === 0) return state;
      const previous = state.past[state.past.length - 1];
      return { past: state.past.slice(0, -1), future: [{ nodes: state.nodes, edges: state.edges }, ...state.future], nodes: previous.nodes, edges: previous.edges };
    }),

    redo: () => set((state) => {
      if (state.future.length === 0) return state;
      const next = state.future[0];
      return { past: [...state.past, { nodes: state.nodes, edges: state.edges }], future: state.future.slice(1), nodes: next.nodes, edges: next.edges };
    }),

    resolveRealId: (id) => {
      const map = get().tempToRealIdMap;
      let currentId = id;
      while (map[currentId]) { currentId = map[currentId]; }
      return currentId;
    },

    enqueueMutation: (mutation) => { set((state) => ({ mutationQueue: [...state.mutationQueue, { ...mutation, id: uuidv4() }] })); debouncedProcessQueue(); },

    processQueue: async () => {
      const state = get();
      if (state.syncStatus === 'saving' || state.mutationQueue.length === 0 || !state.boardId) return;
      if (typeof navigator !== 'undefined' && !navigator.onLine) { set({ syncStatus: 'failed' }); return; }
      set({ syncStatus: 'saving' });
      const queueToProcess = [...state.mutationQueue];
      const successfullyProcessedIds = new Set<string>();
      let hasError = false;
      try {
        for (const mutation of queueToProcess) {
          try {
            switch (mutation.type) {
              case 'CREATE_NODE': {
                const res = await apiClient.post(`/boards/${state.boardId}/elements/nodes`, mutation.payload);
                if (mutation.tempId) {
                  set((s) => ({ tempToRealIdMap: { ...s.tempToRealIdMap, [mutation.tempId!]: res.data.id } }));
                  const { yNodes } = get();
                  const node = yNodes.get(mutation.tempId);
                  if (node) { yNodes.delete(mutation.tempId); yNodes.set(res.data.id, { ...node, id: res.data.id }); }
                }
                break;
              }
              case 'UPDATE_NODE': {
                const realId = get().resolveRealId(mutation.tempId!);
                await apiClient.put(`/boards/${state.boardId}/elements/nodes/${realId}`, mutation.payload);
                break;
              }
              case 'DELETE_NODE': {
                const realId = get().resolveRealId(mutation.tempId!);
                if (!realId.startsWith('temp-')) await apiClient.delete(`/boards/${state.boardId}/elements/nodes/${realId}`);
                break;
              }
              case 'CREATE_EDGE': {
                const req = mutation.payload as CreateEdgeRequest;
                const finalReq = { ...req, sourceNodeId: get().resolveRealId(req.sourceNodeId), targetNodeId: get().resolveRealId(req.targetNodeId) };
                const res = await apiClient.post(`/boards/${state.boardId}/elements/edges`, finalReq);
                if (mutation.tempId) {
                  set((s) => ({ tempToRealIdMap: { ...s.tempToRealIdMap, [mutation.tempId!]: res.data.id } }));
                  const { yEdges } = get();
                  const edge = yEdges.get(mutation.tempId);
                  if (edge) { yEdges.delete(mutation.tempId); yEdges.set(res.data.id, { ...edge, id: res.data.id }); }
                }
                break;
              }
              case 'DELETE_EDGE': {
                const realId = get().resolveRealId(mutation.tempId!);
                if (!realId.startsWith('temp-')) await apiClient.delete(`/boards/${state.boardId}/elements/edges/${realId}`);
                break;
              }
              case 'UPDATE_WHITEBOARD': {
                await apiClient.put(`/boards/${state.boardId}/whiteboard`, mutation.payload);
                break;
              }
            }
            successfullyProcessedIds.add(mutation.id);
          } catch (error) { hasError = true; break; }
        }
      } finally {
        set((state) => ({ mutationQueue: state.mutationQueue.filter(m => !successfullyProcessedIds.has(m.id)), syncStatus: hasError ? 'failed' : 'saved' }));
        if (get().mutationQueue.length > 0 && !hasError) debouncedProcessQueue();
      }
    },

    onNodesChange: (changes) => {
      const { previewNodes, nodes, yNodes } = get();
      const previewChanges = changes.filter(c => 'id' in c && c.id.startsWith('preview-'));
      const realChanges = changes.filter(c => !('id' in c) || !c.id.startsWith('preview-'));
      if (previewChanges.length > 0) set({ previewNodes: applyNodeChanges(previewChanges, previewNodes) });
      if (realChanges.length > 0) {
        const updatedNodes = applyNodeChanges(realChanges, nodes);
        get().updateSelection(updatedNodes.filter(n => n.selected).map(n => n.id));
        set({ nodes: updatedNodes });

        // Sync changes back to Yjs (stripping selection state)
        realChanges.forEach(change => {
            if (change.type === 'position') {
                const node = updatedNodes.find(n => n.id === change.id);
                if (node) {
                    const { selected, ...nodeToSave } = node;
                    yNodes.set(change.id, nodeToSave);
                }
            }
            if (change.type === 'dimensions' && change.dimensions) {
                const node = yNodes.get(change.id);
                if (node) {
                    const metadata = typeof node.data === 'string' ? JSON.parse(node.data) : node.data;
                    
                    // Only update if dimensions actually changed significantly to prevent loops
                    if (Math.abs((metadata.width || 0) - change.dimensions.width) > 1 || 
                        Math.abs((metadata.height || 0) - change.dimensions.height) > 1) {
                        
                        const updatedData = { ...metadata, width: change.dimensions.width, height: change.dimensions.height };
                        const updatedNode = { ...node, id: change.id, data: updatedData };
                        const { selected, ...nodeToSave } = updatedNode;
                        yNodes.set(change.id, nodeToSave);
                        get().saveNodePosition(updatedNode); // Persist to backend
                    }
                }
            }
            if (change.type === 'remove') {
                if (yNodes.has(change.id)) {
                    yNodes.delete(change.id);
                    get().enqueueMutation({ type: 'DELETE_NODE', tempId: change.id, payload: {} });
                }
            }
        });
      }
    },
    
    onEdgesChange: (changes) => {
      const { previewEdges, edges, yEdges } = get();
      const previewChanges = changes.filter(c => 'id' in c && c.id.startsWith('preview-'));
      const realChanges = changes.filter(c => !('id' in c) || !c.id.startsWith('preview-'));
      if (previewChanges.length > 0) set({ previewEdges: applyEdgeChanges(previewChanges, previewEdges) });
      if (realChanges.length > 0) {
        const updatedEdges = applyEdgeChanges(realChanges, edges);
        set({ edges: updatedEdges });

        realChanges.forEach(change => {
            if (change.type === 'remove') {
                if (yEdges.has(change.id)) {
                    yEdges.delete(change.id);
                    get().enqueueMutation({ type: 'DELETE_EDGE', tempId: change.id, payload: {} });
                }
            }
        });
      }
    },

    onConnect: (connection) => {
      const { yEdges } = get();
      get().commitHistory();
      if (!connection.source || !connection.target) return;
      const tempId = `temp-${uuidv4()}`;
      const tempEdge: Edge = { 
        id: tempId, 
        source: connection.source, 
        target: connection.target,
        sourceHandle: connection.sourceHandle || undefined,
        targetHandle: connection.targetHandle || undefined,
        markerEnd: { type: MarkerType.ArrowClosed, color: '#3b82f6' }
      };
      yEdges.set(tempId, tempEdge);
      get().enqueueMutation({ 
        type: 'CREATE_EDGE', 
        tempId, 
        payload: { 
          sourceNodeId: connection.source, 
          targetNodeId: connection.target, 
          metadata: JSON.stringify({ sourceHandle: connection.sourceHandle, targetHandle: connection.targetHandle }) 
        } 
      });
    },

    addNode: (type, position, data) => {
      const { yNodes } = get();
      console.log('Adding node of type:', type, 'at position:', position);
      get().commitHistory();
      const tempId = `temp-${uuidv4()}`;
      const nodeData = { label: `New ${type}`, ...data };
      const tempNode: Node = { id: tempId, type: type.toLowerCase(), position, data: nodeData };
      yNodes.set(tempId, tempNode);
      get().enqueueMutation({ type: 'CREATE_NODE', tempId, payload: { type, x: position.x, y: position.y, metadata: JSON.stringify(nodeData) } });
    },

    updateNodeLabel: (id: string, label: string) => {
      const { yNodes } = get();
      const node = yNodes.get(id);
      if (node) {
        const updatedNode = { ...node, data: { ...node.data, label } };
        const { selected, ...nodeToSave } = updatedNode;
        yNodes.set(id, nodeToSave);
        
        const realId = get().resolveRealId(id);
        if (!realId.startsWith('temp-')) {
          get().enqueueMutation({ 
            type: 'UPDATE_NODE', 
            payload: { id: realId, type: updatedNode.type, x: updatedNode.position.x, y: updatedNode.position.y, metadata: JSON.stringify(updatedNode.data) } 
          });
        }
      }
    },

    updateNodeColor: (id: string, color: string) => {
      const { yNodes } = get();
      const node = yNodes.get(id);
      if (node) {
        const updatedNode = { ...node, data: { ...node.data, color } };
        const { selected, ...nodeToSave } = updatedNode;
        yNodes.set(id, nodeToSave);
        
        const realId = get().resolveRealId(id);
        if (!realId.startsWith('temp-')) {
          get().enqueueMutation({ 
            type: 'UPDATE_NODE', 
            payload: { id: realId, type: updatedNode.type, x: updatedNode.position.x, y: updatedNode.position.y, metadata: JSON.stringify(updatedNode.data) } 
          });
        }
      }
    },

    saveNodePosition: (node) => {
      const { yNodes } = get();
      if (node.id.startsWith('preview-')) return;
      const { selected, ...nodeToSave } = node;
      yNodes.set(node.id, nodeToSave);
      get().enqueueMutation({ type: 'UPDATE_NODE', tempId: node.id, payload: { x: node.position.x, y: node.position.y, metadata: JSON.stringify(node.data) } });
    },

    deleteElements: (nodesToDelete, edgesToDelete) => {
      const { yNodes, yEdges, previewNodes, previewEdges } = get();
      const pNodesToDelete = nodesToDelete.filter(n => n.id.startsWith('preview-'));
      const pEdgesToDelete = edgesToDelete.filter(e => e.id.startsWith('preview-'));
      if (pNodesToDelete.length > 0 || pEdgesToDelete.length > 0) {
        set({ previewNodes: previewNodes.filter(n => !pNodesToDelete.some(pd => pd.id === n.id)), previewEdges: previewEdges.filter(e => !pEdgesToDelete.some(ed => ed.id === e.id)) });
      }
      const rNodesToDelete = nodesToDelete.filter(n => !n.id.startsWith('preview-'));
      const rEdgesToDelete = edgesToDelete.filter(e => !e.id.startsWith('preview-'));
      if (rNodesToDelete.length > 0 || rEdgesToDelete.length > 0) {
        get().commitHistory();
        rNodesToDelete.forEach(node => {
          if (yNodes.has(node.id)) {
            yNodes.delete(node.id);
            get().enqueueMutation({ type: 'DELETE_NODE', tempId: node.id, payload: {} });
          }
        });
        rEdgesToDelete.forEach(edge => {
          if (yEdges.has(edge.id)) {
            yEdges.delete(edge.id);
            get().enqueueMutation({ type: 'DELETE_EDGE', tempId: edge.id, payload: {} });
          }
        });
      }
    },

    copySelection: (selectedNodes, selectedEdges) => {
      if (selectedNodes.length === 0) return;
      set({ clipboard: { nodes: JSON.parse(JSON.stringify(selectedNodes)), edges: JSON.parse(JSON.stringify(selectedEdges)) } });
      toast.info('Copied selection');
    },

    pasteSelection: (position) => {
      const { clipboard, yNodes, yEdges } = get();
      if (!clipboard) return;
      get().commitHistory();
      const nodeIdMap: Record<string, string> = {};
      let offsetX = 20, offsetY = 20;
      if (position && clipboard.nodes.length > 0) { offsetX = position.x - clipboard.nodes[0].position.x; offsetY = position.y - clipboard.nodes[0].position.y; }
      clipboard.nodes.map(node => {
        const newId = `temp-${uuidv4()}`;
        nodeIdMap[node.id] = newId;
        const newNode: Node = { ...node, id: newId, selected: true, position: { x: node.position.x + offsetX, y: node.position.y + offsetY } };
        yNodes.set(newId, newNode);
        get().enqueueMutation({ type: 'CREATE_NODE', tempId: newId, payload: { type: newNode.type, x: newNode.position.x, y: newNode.position.y, metadata: JSON.stringify(newNode.data) } });
        return newNode;
      });
      clipboard.edges.filter(edge => nodeIdMap[edge.source] && nodeIdMap[edge.target]).forEach(edge => {
        const newId = `temp-${uuidv4()}`;
        const newEdge: Edge = { ...edge, id: newId, source: nodeIdMap[edge.source], target: nodeIdMap[edge.target], selected: true };
        yEdges.set(newId, newEdge);
        get().enqueueMutation({ type: 'CREATE_EDGE', tempId: newId, payload: { sourceNodeId: newEdge.source, targetNodeId: newEdge.target, metadata: '{}' } });
      });
      toast.success(`Pasted ${clipboard.nodes.length} nodes`);
    },

    selectAll: () => {
      const { nodes, edges } = get();
      set({
        nodes: nodes.map(n => ({ ...n, selected: true })),
        edges: edges.map(e => ({ ...e, selected: true }))
      });
    },

    saveVersion: async (name, description) => {
      const { boardId, nodes, edges, yDoc, boardType } = get();
      if (!boardId) return;
      try {
        const req = { name, description, nodesData: boardType === 'Whiteboard' ? '[]' : JSON.stringify(nodes), edgesData: boardType === 'Whiteboard' ? '[]' : JSON.stringify(edges), yjsState: Array.from(Y.encodeStateAsUpdate(yDoc)) };
        await apiClient.post(`/boards/${boardId}/versions`, req);
        toast.success('Version saved');
      } catch (error) { toast.error('Failed to save version'); }
    },

    restoreVersion: async (versionId) => {
      const { boardId } = get();
      if (!boardId) return;
      try {
        await apiClient.post(`/boards/${boardId}/versions/${versionId}/restore`);
        toast.success('Version restored');
        window.location.reload();
      } catch (error) { toast.error('Failed to restore version'); }
    },

    fetchVersions: async () => {
      const { boardId } = get();
      if (!boardId) return [];
      try {
        const res = await apiClient.get<BoardVersionResponse[]>(`/boards/${boardId}/versions`);
        return res.data;
      } catch (error) { toast.error('Failed to fetch versions'); return []; }
    },

    generateAiDiagram: async (prompt, type, templateId) => {
      const { boardId, commitHistory } = get();
      if (!boardId) return;
      set({ isGeneratingAi: true, aiStatus: 'Thinking...' });
      commitHistory();
      try {
        const typeMap: Record<string, number> = { 'Flowchart': 1, 'Mindmap': 2, 'SystemArchitecture': 3, 'UserJourney': 4 };
        await apiClient.post(`/boards/${boardId}/ai/generate`, { prompt, type: typeMap[type] || 1, templateId });
        await get().loadViewportElements(0, 0, 2000, 2000); 
        toast.success('Diagram generated by AI');
        await get().fetchAiHistory();
      } catch (error: any) { toast.error(error.response?.data?.detail || 'AI generation failed'); }
      finally { set({ isGeneratingAi: false, aiStatus: '' }); }
    },

    previewAiDiagram: async (prompt, type, templateId, refinementCommand) => {
      set({ isGeneratingAi: true, aiStatus: refinementCommand ? 'Analyzing...' : 'Architecting...' });
      const { nodes, edges, boardId } = get();
      try {
        const typeMap: Record<string, number> = { 'Flowchart': 1, 'Mindmap': 2, 'SystemArchitecture': 3, 'UserJourney': 4 };
        const commandMap: Record<string, number> = { 'ImproveArchitecture': 1, 'AddMissingComponents': 2, 'SimplifyDiagram': 3, 'ExplainDiagram': 4 };
        const payload = { 
          prompt, type: typeMap[type] || 1, templateId,
          refinementCommand: refinementCommand ? (commandMap[refinementCommand] || null) : null,
          existingNodes: refinementCommand ? nodes.map(n => ({ id: n.id, type: n.type, position: { x: n.position.x, y: n.position.y }, data: { label: (n.data as any).label }, metadata: (n.data as any).metadata || "{}" })) : null,
          existingEdges: refinementCommand ? edges.map(e => ({ id: e.id, source: e.source, target: e.target })) : null
        };
        const response = await apiClient.post<DiagramResponse>(`/boards/${boardId}/ai/preview`, payload);
        const isExplanation = refinementCommand === 'ExplainDiagram';
        set({
          previewNodes: response.data.nodes.map(n => ({ id: `preview-${n.id}`, type: n.type.toLowerCase(), position: { x: n.x, y: n.y }, data: { label: n.type, metadata: n.metadata }, style: isExplanation ? { width: 400, background: '#fefce8', border: '2px solid #eab308', padding: 20 } : { opacity: 0.8, border: '2px dashed #3b82f6', background: '#eff6ff' } })),
          previewEdges: response.data.edges.map(e => ({ id: `preview-${e.id}`, source: `preview-${e.sourceNodeId}`, target: `preview-${e.targetNodeId}`, animated: true, style: { stroke: '#3b82f6', strokeDasharray: '5,5' } })),
          showCurrentInPreview: true
        });
        toast.info(isExplanation ? 'AI Explanation ready.' : 'Refinement preview generated.');
      } catch (error: any) { toast.error(error.response?.data?.detail || 'AI operation failed'); }
      finally { set({ isGeneratingAi: false, aiStatus: '' }); }
    },

    acceptAiPreview: () => {
      const { previewNodes, previewEdges, yNodes, yEdges, boardId } = get();
      if (!boardId) return;
      get().commitHistory();
      const idMap: Record<string, string> = {};
      previewNodes.forEach(pn => {
        const finalId = `temp-${uuidv4()}`;
        idMap[pn.id] = finalId;
        const node: Node = { ...pn, id: finalId, style: undefined };
        yNodes.set(finalId, node);
        get().enqueueMutation({ type: 'CREATE_NODE', tempId: finalId, payload: { type: node.type, x: node.position.x, y: node.position.y, metadata: JSON.stringify(node.data) } });
      });
      previewEdges.forEach(pe => {
        const sourceId = idMap[pe.source], targetId = idMap[pe.target];
        if (sourceId && targetId) {
          const realId = `temp-${uuidv4()}`;
          const edge: Edge = { ...pe, id: realId, source: sourceId, target: targetId, animated: false, style: undefined };
          yEdges.set(realId, edge);
          get().enqueueMutation({ type: 'CREATE_EDGE', tempId: realId, payload: { sourceNodeId: sourceId, targetNodeId: targetId, metadata: '{}' } });
        }
      });
      set({ previewNodes: [], previewEdges: [] });
      toast.success('AI diagram accepted');
      get().fetchAiHistory();
    },

    rejectAiPreview: () => { set({ previewNodes: [], previewEdges: [] }); toast.info('AI diagram discarded'); },

    togglePreviewComparison: () => set(s => ({ showCurrentInPreview: !s.showCurrentInPreview })),

    fetchAiHistory: async () => {
      const { boardId } = get();
      if (!boardId) return;
      try {
        const res = await apiClient.get<AiGenerationHistoryResponse[]>(`/boards/${boardId}/ai/history`);
        set({ aiHistory: res.data });
      } catch (error) { console.error('Failed to fetch AI history', error); }
    },

    fetchTemplates: async () => {
      try {
        const res = await apiClient.get<BoardTemplateResponse[]>('/templates');
        set({ templates: res.data });
      } catch (error) { console.error('Failed to fetch templates', error); }
    },

    updateWhiteboard: (records) => {
      const { yWhiteboard, boardId } = get();
      if (!boardId) return;
      const persistentRecords: Record<string, any> = {};
      Object.entries(records).forEach(([id, record]) => {
        if (record === null) yWhiteboard.delete(id);
        else yWhiteboard.set(id, record);
        if (!id.startsWith('user_presence:')) persistentRecords[id] = record;
      });
      if (Object.keys(persistentRecords).length > 0) get().enqueueMutation({ type: 'UPDATE_WHITEBOARD', payload: persistentRecords });
    }
  };
});
