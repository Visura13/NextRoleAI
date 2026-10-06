import { render, screen } from '@testing-library/react';
import useSWR from 'swr';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApplicationList } from '../../api/types';
import { RecruiterApplicationsPage } from './ApplicationsPage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  api: { request: vi.fn() },
  ApiError: class ApiError extends Error { status = 400; },
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
    status: 'Submitted',
    sourceWorkflowRunId: null,
    statusHistory: [{
      id: 'event-1',
      previousStatus: null,
      newStatus: 'Submitted',
      actorRole: 'JobSeeker',
      note: 'Application submitted.',
      createdAtUtc: '2030-01-01T00:00:00Z',
    }],
    notificationDeliveries: [{
      eventType: 'ApplicationSubmitted',
      provider: 'Resend',
      status: 'Skipped',
      attemptCount: 1,
      createdAtUtc: '2030-01-01T00:00:00Z',
      attemptedAtUtc: '2030-01-01T00:00:01Z',
    }],
    createdAtUtc: '2030-01-01T00:00:00Z',
    updatedAtUtc: '2030-01-01T00:00:00Z',
  }],
};

describe('RecruiterApplicationsPage', () => {
  beforeEach(() => {
    vi.mocked(useSWR).mockReturnValue({
      data: applications,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never);
  });

  it('shows candidate evidence, decision controls, and notification audit', () => {
    render(<RecruiterApplicationsPage />);

    expect(screen.getByRole('heading', { name: 'Alex Morgan' })).toBeInTheDocument();
    expect(screen.getByText('Resend · Skipped')).toBeInTheDocument();
    expect(screen.getByLabelText('Decision')).toHaveValue('InReview');
    expect(screen.getByRole('button', { name: 'Record decision' })).toBeDisabled();
  });
});
