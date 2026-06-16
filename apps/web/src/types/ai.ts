import { DiagramResponse } from './canvas';

export interface AiGenerationHistoryResponse {
  id: string;
  boardId: string;
  prompt: string;
  diagramType: string;
  result: DiagramResponse;
  createdAt: string;
  creatorName: string;
}
