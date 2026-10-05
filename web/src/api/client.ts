import type {
  AuthenticationResponse,
  AuthSession,
  CurrentUser,
  ProblemDetails,
} from './types';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5251').replace(/\/$/, '');
const STORAGE_KEY = 'nextroleai.auth.v1';

export class ApiError extends Error {
  readonly status: number;
  readonly errors: Record<string, string[]>;

  constructor(
    message: string,
    status: number,
    errors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}

function toSession(response: AuthenticationResponse): AuthSession {
  const { accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc, ...user } = response;
  return { user, accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc };
}

function readStoredSession(): AuthSession | null {
  const value = sessionStorage.getItem(STORAGE_KEY);
  if (!value) return null;

  try {
    return JSON.parse(value) as AuthSession;
  } catch {
    sessionStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

class ApiClient {
  private session = readStoredSession();
  private refreshRequest: Promise<boolean> | null = null;
  private listeners = new Set<(session: AuthSession | null) => void>();

  getSession() {
    return this.session;
  }

  subscribe(listener: (session: AuthSession | null) => void) {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  }

  setAuthentication(response: AuthenticationResponse) {
    this.setSession(toSession(response));
  }

  clearSession() {
    this.setSession(null);
  }

  async request<T>(path: string, init: RequestInit = {}, authenticated = false): Promise<T> {
    const response = await this.fetch(path, init, authenticated);

    if (response.status === 401 && authenticated && await this.refresh()) {
      return this.parse<T>(await this.fetch(path, init, true));
    }

    return this.parse<T>(response);
  }

  async authenticate(path: string, payload: unknown) {
    const response = await this.request<AuthenticationResponse>(path, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
    this.setAuthentication(response);
    return response;
  }

  async verifySession() {
    if (!this.session) return null;
    const user = await this.request<CurrentUser>('/api/auth/me', {}, true);
    this.setSession({ ...this.session, user });
    return user;
  }

  async logout() {
    const refreshToken = this.session?.refreshToken;
    try {
      if (refreshToken) {
        await this.request<void>('/api/auth/logout', {
          method: 'POST',
          body: JSON.stringify({ refreshToken }),
        });
      }
    } finally {
      this.clearSession();
    }
  }

  private setSession(session: AuthSession | null) {
    this.session = session;
    if (session) sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    else sessionStorage.removeItem(STORAGE_KEY);
    this.listeners.forEach((listener) => listener(session));
  }

  private async fetch(path: string, init: RequestInit, authenticated: boolean) {
    const headers = new Headers(init.headers);
    if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
    if (authenticated && this.session) headers.set('Authorization', `Bearer ${this.session.accessToken}`);
    return fetch(`${API_BASE_URL}${path}`, { ...init, headers });
  }

  private async refresh() {
    if (!this.session) return false;
    if (this.refreshRequest) return this.refreshRequest;

    this.refreshRequest = (async () => {
      try {
        const response = await this.fetch('/api/auth/refresh', {
          method: 'POST',
          body: JSON.stringify({ refreshToken: this.session?.refreshToken }),
        }, false);
        const authentication = await this.parse<AuthenticationResponse>(response);
        this.setAuthentication(authentication);
        return true;
      } catch {
        this.clearSession();
        return false;
      } finally {
        this.refreshRequest = null;
      }
    })();

    return this.refreshRequest;
  }

  private async parse<T>(response: Response): Promise<T> {
    if (response.ok) {
      if (response.status === 204) return undefined as T;
      return response.json() as Promise<T>;
    }

    let problem: ProblemDetails = {};
    try {
      problem = await response.json() as ProblemDetails;
    } catch {
      // A useful generic message is returned when the server has no JSON body.
    }

    const firstValidationMessage = Object.values(problem.errors ?? {})[0]?.[0];
    throw new ApiError(
      firstValidationMessage ?? problem.detail ?? problem.title ?? 'The request could not be completed.',
      response.status,
      problem.errors,
    );
  }
}

export const api = new ApiClient();
export const swrFetcher = <T>(path: string) => api.request<T>(path, {}, true);
export const publicFetcher = <T>(path: string) => api.request<T>(path);
