import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, ApiError } from '../api/client';
import { AuthContext, type AuthContextValue } from './authContextValue';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState(api.getSession());
  const [isReady, setIsReady] = useState(!api.getSession());

  useEffect(() => api.subscribe(setSession), []);

  useEffect(() => {
    if (!api.getSession()) return;
    api.verifySession()
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 401) api.clearSession();
      })
      .finally(() => setIsReady(true));
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    session,
    isReady,
    async login(email, password) {
      await api.authenticate('/api/auth/login', { email, password });
    },
    async register(input) {
      const endpoint = input.role === 'JobSeeker'
        ? '/api/auth/register/job-seeker'
        : '/api/auth/register/recruiter';
      const { role: _role, ...registration } = input;
      await api.authenticate(endpoint, registration);
    },
    logout: () => api.logout(),
  }), [isReady, session]);

  return <AuthContext value={value}>{children}</AuthContext>;
}
