export interface BoardVersionResponse {
  id: string;
  boardId: string;
  name?: string;
  description?: string;
  createdAt: string;
  createdBy: string;
  creatorName: string;
  type: number;
}

export interface VersionDetailResponse {
  id: string;
  nodesData: string;
  edgesData: string;
  yjsState: number[];
}
