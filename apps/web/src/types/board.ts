export type BoardType = 'Flowchart' | 'Mindmap' | 'Whiteboard' | 'Wireframe';

export interface Board {
  id: string;
  name: string;
  type: BoardType;
  workspaceId: string;
  createdAt: string;
}

export interface CreateBoardInput {
  name: string;
  type: BoardType;
}
