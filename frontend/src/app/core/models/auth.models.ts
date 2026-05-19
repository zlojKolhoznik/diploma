export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string | null;
  idToken: string | null;
  refreshToken: string | null;
  expiresIn: number;
}

export interface JwtPayload {
  sub: string;
  email?: string;
  given_name?: string;
  family_name?: string;
  'cognito:groups'?: string[];
  client_id?: string;
  token_use?: string;
  exp: number;
  iat: number;
}

