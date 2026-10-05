import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { describe, expect, it, vi } from 'vitest';
import { AuthContext, type AuthContextValue } from './authContextValue';
import { ProtectedRoute } from './ProtectedRoute';

function authValue(role: 'JobSeeker' | 'Recruiter' | null): AuthContextValue {
  return {
    isReady: true,
    session: role ? {
      user: { userId: 'user-1', email: 'user@example.com', firstName: 'Asha', lastName: 'Perera', role },
      accessToken: 'access', accessTokenExpiresAtUtc: '2026-10-05T11:00:00Z',
      refreshToken: 'refresh', refreshTokenExpiresAtUtc: '2026-10-12T10:00:00Z',
    } : null,
    login: vi.fn(), register: vi.fn(), logout: vi.fn(),
  };
}

function renderRoute(value: AuthContextValue) {
  return render(
    <AuthContext value={value}>
      <MemoryRouter initialEntries={['/recruiter']}>
        <Routes>
          <Route path="/login" element={<p>Login page</p>} />
          <Route path="/dashboard" element={<p>Dashboard redirect</p>} />
          <Route element={<ProtectedRoute role="Recruiter" />}>
            <Route path="/recruiter" element={<p>Recruiter workspace</p>} />
          </Route>
        </Routes>
      </MemoryRouter>
    </AuthContext>,
  );
}

describe('ProtectedRoute', () => {
  it('sends signed-out users to login', () => {
    renderRoute(authValue(null));
    expect(screen.getByText('Login page')).toBeInTheDocument();
  });

  it('allows the required role', () => {
    renderRoute(authValue('Recruiter'));
    expect(screen.getByText('Recruiter workspace')).toBeInTheDocument();
  });

  it('keeps another role out of the recruiter workspace', () => {
    renderRoute(authValue('JobSeeker'));
    expect(screen.getByText('Dashboard redirect')).toBeInTheDocument();
  });
});
