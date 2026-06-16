export interface NodeResponse {
  id: string;
  boardId: string;
  type: string;
  x: number;
  y: number;
  width?: number;
  height?: number;
  metadata: string;
  version: number;
}

export interface EdgeResponse {
  id: string;
  boardId: string;
  sourceNodeId: string;
  targetNodeId: string;
  metadata: string;
}

export interface DiagramResponse {
  nodes: NodeResponse[];
  edges: EdgeResponse[];
}

export interface WhiteboardResponse {
  records: Record<string, any>;
}

export interface CreateNodeRequest {
  type: string;
  x: number;
  y: number;
  width?: number;
  height?: number;
  metadata: string;
}

export interface UpdateNodeRequest {
  x: number;
  y: number;
  width?: number;
  height?: number;
  metadata: string;
}

export interface CreateEdgeRequest {
  sourceNodeId: string;
  targetNodeId: string;
  metadata: string;
}
