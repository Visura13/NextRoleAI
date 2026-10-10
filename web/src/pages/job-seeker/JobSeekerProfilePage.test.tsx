import { cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import useSWR from 'swr';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { api } from '../../api/client';
import type { JobSeekerProfile } from '../../api/types';
import { JobSeekerProfilePage } from './JobSeekerProfilePage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  api: { request: vi.fn() },
  ApiError: class ApiError extends Error { status = 400; },
  swrFetcher: vi.fn(),
}));

const profile: JobSeekerProfile = {
  id: 'profile-1',
  headline: 'Frontend engineer',
  summary: 'Builds accessible React applications.',
  location: 'Colombo',
  preferredJobTitle: 'React Frontend Engineer',
  preferredSalary: 200000,
  yearsOfExperience: 2,
  skills: ['React', 'TypeScript'],
  updatedAtUtc: '2030-01-01T00:00:00Z',
};

describe('JobSeekerProfilePage', () => {
  afterEach(cleanup);

  beforeEach(() => {
    vi.mocked(useSWR).mockImplementation((key) => ({
      data: key === '/api/cv' ? undefined : profile,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never));
    vi.mocked(api.request).mockResolvedValue(profile);
  });

  it('combines CV management and job preferences in one profile workspace', () => {
    render(<MemoryRouter><JobSeekerProfilePage /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'Your professional evidence' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Upload CV' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Tell NextRoleAI what you want next.' })).toBeInTheDocument();
  });

  it('saves preferred salary and redirects to recommendations', async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/job-seeker/profile']}>
        <Routes>
          <Route path="/job-seeker/profile" element={<JobSeekerProfilePage />} />
          <Route path="/job-seeker/recommendations" element={<h1>Recommendations destination</h1>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByLabelText(/Preferred minimum monthly salary/)).toHaveValue(200000);
    await user.click(screen.getByRole('button', { name: 'Save profile' }));

    expect(await screen.findByRole('heading', { name: 'Recommendations destination' })).toBeInTheDocument();
    expect(api.request).toHaveBeenCalledWith(
      '/api/profiles/job-seeker',
      expect.objectContaining({ body: expect.stringContaining('"preferredSalary":200000') }),
      true,
    );
  });
});
