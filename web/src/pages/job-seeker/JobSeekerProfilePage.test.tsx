import { cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import useSWR from 'swr';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { api } from '../../api/client';
import type { CvProfile, JobSeekerProfile } from '../../api/types';
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

const confirmedCv: CvProfile = {
  id: 'cv-1',
  originalFileName: 'candidate.pdf',
  contentType: 'application/pdf',
  sizeBytes: 2048,
  sha256Checksum: '1234567890abcdef',
  status: 'Confirmed',
  candidateName: 'Alex Morgan',
  email: 'alex@example.com',
  phone: '',
  location: 'Colombo',
  currentJobTitle: 'Frontend engineer',
  professionalSummary: 'Builds accessible React applications.',
  yearsExperience: 2,
  skills: ['React', 'TypeScript'],
  education: [],
  qualityAssessment: null,
  analysisMethod: 'ai',
  analysisModel: 'test-model',
  analysisPromptVersion: 'cv-analysis-v1',
  analyzedAtUtc: '2030-01-01T00:00:00Z',
  failureReason: null,
  createdAtUtc: '2030-01-01T00:00:00Z',
  updatedAtUtc: '2030-01-01T00:00:00Z',
};

describe('JobSeekerProfilePage', () => {
  afterEach(cleanup);

  beforeEach(() => {
    vi.mocked(useSWR).mockImplementation((key) => ({
      data: key === '/api/cv' ? confirmedCv : profile,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never));
    vi.mocked(api.request).mockResolvedValue(profile);
  });

  it('combines CV management and job preferences in one profile workspace', () => {
    render(<MemoryRouter><JobSeekerProfilePage /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'Your professional evidence' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Tell NextRoleAI what you want next.' })).toBeInTheDocument();
    expect(screen.queryByLabelText('Professional headline')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Skills')).not.toBeInTheDocument();
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
    await user.click(screen.getByRole('button', { name: 'Save preferences and view recommendations' }));

    expect(await screen.findByRole('heading', { name: 'Recommendations destination' })).toBeInTheDocument();
    expect(api.request).toHaveBeenCalledWith(
      '/api/profiles/job-seeker',
      expect.objectContaining({
        body: expect.stringContaining('"preferredSalary":200000'),
      }),
      true,
    );
    expect(api.request).toHaveBeenCalledWith(
      '/api/profiles/job-seeker',
      expect.objectContaining({
        body: expect.stringContaining('"skills":["React","TypeScript"]'),
      }),
      true,
    );
  });
});
