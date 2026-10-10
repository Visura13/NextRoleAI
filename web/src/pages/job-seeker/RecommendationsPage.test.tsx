import { cleanup, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import useSWR from 'swr';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Job, RecommendationResponse } from '../../api/types';
import { RecommendationsPage } from './RecommendationsPage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  ApiError: class ApiError extends Error {
    readonly status: number;

    constructor(message: string, status: number) {
      super(message);
      this.status = status;
    }
  },
  swrFetcher: vi.fn(),
}));

const reactJob: Job = {
  id: 'job-react',
  companyId: 'company-1',
  companyName: 'Example Company',
  title: 'React Frontend Engineer',
  description: 'Build accessible React interfaces with TypeScript.',
  location: 'Colombo',
  employmentType: 'FullTime',
  workMode: 'Hybrid',
  minimumYearsExperience: 1,
  salaryMinimum: null,
  salaryMaximum: null,
  salaryCurrency: null,
  status: 'Published',
  publishedAtUtc: '2030-01-01T00:00:00Z',
  closesAtUtc: null,
  skills: [
    { name: 'React', isRequired: true },
    { name: 'TypeScript', isRequired: true },
  ],
  createdAtUtc: '2030-01-01T00:00:00Z',
  updatedAtUtc: '2030-01-01T00:00:00Z',
};

const response: RecommendationResponse = {
  items: [{
    job: reactJob,
    score: 93,
    breakdown: {
      skillsFit: 34,
      roleFit: 24,
      experienceFit: 12,
      educationFit: 9,
      locationFit: 10,
      salaryFit: 4,
      total: 93,
    },
    matchedSkills: ['React', 'TypeScript'],
    missingRequiredSkills: [],
    reasons: [
      'React and TypeScript directly support the frontend responsibilities.',
      'The candidate has relevant web application experience.',
    ],
    algorithmVersion: 'ai-semantic-ranking-v1:test-model',
  }],
};

describe('RecommendationsPage', () => {
  afterEach(cleanup);

  beforeEach(() => {
    vi.mocked(useSWR).mockReturnValue({
      data: response,
      error: undefined,
      isLoading: false,
    } as never);
  });

  it('shows the AI semantic ranking and its validated score evidence', () => {
    render(<MemoryRouter><RecommendationsPage /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'Jobs ranked against your complete professional profile.' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'React Frontend Engineer' })).toBeInTheDocument();
    expect(screen.getByText('93.0')).toBeInTheDocument();
    expect(screen.queryByText(/Algorithm version:/)).not.toBeInTheDocument();
    const breakdown = screen.getByLabelText('Score breakdown');
    expect(breakdown).toHaveTextContent('Skills fit 34/35');
    expect(breakdown).toHaveTextContent('Role fit 24/25');
    expect(breakdown).toHaveTextContent('Education fit 9/10');
    expect(breakdown).toHaveTextContent('Salary fit 4/5');
    expect(screen.getByText('React and TypeScript directly support the frontend responsibilities.')).toBeInTheDocument();
  });
});
