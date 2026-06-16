export interface ShareLinkResponse {
  id: string;
  token: string;
  role: number;
  expiresAt?: string;
  isRevoked: boolean;
  createdAt: string;
  url: string;
}

export interface BoardSharingInfoResponse {
  boardId: string;
  visibility: number;
  permissions: BoardPermissionResponse[];
  shareLinks: ShareLinkResponse[];
}

export interface BoardPermissionResponse {
  userId: string;
  userEmail: string;
  displayName: string;
  role: number;
}
