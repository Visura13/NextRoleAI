import { createContext } from 'react';
import type { AuthSession, UserRole } from '../api/types';

export interface RegisterInput {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface AuthContextValue {
  session: AuthSession | null;
  isReady: boolean;
  login(email: string, password: string): Promise<void>;
  register(input: RegisterInput): Promise<void>;
  logout(): Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
