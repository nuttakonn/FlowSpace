export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  id: string;
  email: string;
  displayName: string;
  accessToken: string;
  refreshToken: string;
}
