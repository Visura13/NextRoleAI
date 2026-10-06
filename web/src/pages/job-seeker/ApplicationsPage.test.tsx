import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import useSWR from 'swr';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApplicationList } from '../../api/types';
import { ApplicationsPage } from './ApplicationsPage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  api: { request: vi.fn() },
  ApiError: class ApiError extends Error { status = 400; },
  publicFetcher: vi.fn(),
  swrFetcher: vi.fn(),
}));

const applications: ApplicationList = {
  page: 1,
  pageSize: 100,
  totalCount: 1,
  totalPages: 1,
  items: [{
    id: 'application-1',
    jobPostingId: 'job-1',
    jobTitle: 'Platform Engineer',
    companyName: 'Northstar Labs',
    jobLocation: 'Colombo',
    jobSeekerUserId: 'user-1',
    jobSeekerName: 'Alex Morgan',
    jobSeekerEmail: 'alex@example.com',
    coverNote: 'I have relevant platform engineering experience.',
    status: 'MoreInformationRequested',
    sourceWorkflowRunId: null,
    statusHistory: [{
      id: 'event-1',
      previousStatus: 'InReview',
      newStatus: 'MoreInformationRequested',
      actorRole: 'Recruiter',
      note: 'Please share an incident example.',
      createdAtUtc: '2030-01-02T00:00:00Z',
    }],
    notificationDeliveries: [],
    createdAtUtc: '2030-01-01T00:00:00Z',
    updatedAtUtc: '2030-01-02T00:00:00Z',
  }],
};

describe('ApplicationsPage', () => {
  beforeEach(() => {
    vi.mocked(useSWR).mockReturnValue({
      data: applications,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never);
  });

  it('shows cross-platform status and the requested-information action', () => {
    render(<MemoryRouter initialEntries={['/job-seeker/applications']}><ApplicationsPage /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'Platform Engineer' })).toBeInTheDocument();
    expect(screen.getByText('More Information Requested')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Send requested information' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Withdraw application' })).toBeDisabled();
  });
});
