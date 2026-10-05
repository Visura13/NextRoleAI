import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { describe, expect, it, vi } from 'vitest';
import { AuthContext } from '../../auth/authContextValue';
import { RegisterPage } from './RegisterPage';

describe('RegisterPage', () => {
  it('validates required fields before calling the API', async () => {
    const register = vi.fn();
    render(
      <AuthContext value={{ session: null, isReady: true, login: vi.fn(), register, logout: vi.fn() }}>
        <MemoryRouter><RegisterPage /></MemoryRouter>
      </AuthContext>,
    );

    await userEvent.click(screen.getByRole('button', { name: /create job seeker account/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Complete every field');
    expect(register).not.toHaveBeenCalled();
  });
});
