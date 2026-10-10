import { cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import useSWR from 'swr';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { api } from '../../api/client';
import type { CvProfile } from '../../api/types';
import { CvPage } from './CvPage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  api: { request: vi.fn() },
  ApiError: class ApiError extends Error { status = 400; },
  swrFetcher: vi.fn(),
}));

const aiProfile: CvProfile = {
  id: 'cv-1',
  originalFileName: 'candidate.pdf',
  contentType: 'application/pdf',
  sizeBytes: 2048,
  sha256Checksum: '1234567890abcdef',
  status: 'NeedsReview',
  candidateName: 'Alex Morgan',
  email: 'alex@example.com',
  phone: '+94 77 123 4567',
  location: 'Colombo',
  currentJobTitle: 'Software Engineer',
  professionalSummary: 'Software engineer building reliable web applications.',
  yearsExperience: 3,
  skills: ['React', 'TypeScript', 'C#'],
  education: [{
    qualification: 'BSc (Hons)',
    fieldOfStudy: 'Software Engineering',
    institution: 'SLIIT',
    status: 'In progress',
    evidence: 'BSc (Hons) in Software Engineering at SLIIT',
    confidence: 0.96,
  }],
  qualityAssessment: {
    overallScore: 82,
    completenessScore: 80,
    clarityScore: 85,
    skillsEvidenceScore: 76,
    impactScore: 78,
    atsReadabilityScore: 91,
    strengths: ['Clear technical skills section'],
    improvements: ['Add measurable outcomes to project bullets'],
  },
  analysisMethod: 'ai',
  analysisModel: 'test-model',
  analysisPromptVersion: 'cv-analysis-v1',
  analyzedAtUtc: '2030-01-01T00:00:00Z',
  failureReason: null,
  createdAtUtc: '2030-01-01T00:00:00Z',
  updatedAtUtc: '2030-01-01T00:00:00Z',
};

describe('CvPage', () => {
  afterEach(cleanup);

  beforeEach(() => {
    vi.mocked(useSWR).mockReturnValue({
      data: aiProfile,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never);
    vi.mocked(api.request).mockResolvedValue({ ...aiProfile, status: 'Confirmed' });
  });

  it('shows the AI quality assessment and editable higher education', () => {
    render(<MemoryRouter><CvPage /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'CV quality review' })).toBeInTheDocument();
    expect(screen.getByLabelText('Overall CV quality score 82 out of 100')).toBeInTheDocument();
    expect(screen.getByText('Clear technical skills section')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Education' })).toBeInTheDocument();
    expect(screen.getByDisplayValue('BSc (Hons)')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Software Engineering')).toBeInTheDocument();
    expect(screen.getByText(/O\/L and A\/L entries are excluded/)).toBeInTheDocument();
  });

  it('explains when only deterministic extraction was used', () => {
    vi.mocked(useSWR).mockReturnValue({
      data: { ...aiProfile, qualityAssessment: null, analysisMethod: 'deterministic-fallback', analysisModel: null, education: [] },
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never);

    render(<MemoryRouter><CvPage /></MemoryRouter>);

    expect(screen.getByText(/Basic extraction is active/)).toBeInTheDocument();
    expect(screen.getByText(/No higher-education qualification was extracted/)).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: 'CV quality review' })).not.toBeInTheDocument();
  });

  it('redirects to recommendations after confirming the CV profile', async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/job-seeker/cv']}>
        <Routes>
          <Route path="/job-seeker/cv" element={<CvPage />} />
          <Route path="/job-seeker/recommendations" element={<h1>Recommendations destination</h1>} />
        </Routes>
      </MemoryRouter>,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm CV profile' }));

    expect(await screen.findByRole('heading', { name: 'Recommendations destination' })).toBeInTheDocument();
  });
});
