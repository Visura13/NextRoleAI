import { render, screen } from '@testing-library/react';
import useSWR from 'swr';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { AgentWorkflowList } from '../../api/types';
import { AgentWorkflowsPage } from './AgentWorkflowsPage';

vi.mock('swr', () => ({ default: vi.fn() }));
vi.mock('../../api/client', () => ({
  api: { request: vi.fn() },
  ApiError: class ApiError extends Error { status = 400; },
  swrFetcher: vi.fn(),
}));

const workflowList: AgentWorkflowList = {
  page: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
  items: [{
    id: 'workflow-1',
    objective: 'Find the best 3 backend engineering jobs for my confirmed profile.',
    status: 'PendingApproval',
    approvalStatus: 'Pending',
    currentAgent: 'Human Approval Gate',
    revisionNumber: 0,
    failureCode: null,
    failureMessage: null,
    finalSummary: 'One job passed validation.',
    createdAtUtc: '2030-01-01T00:00:00Z',
    updatedAtUtc: '2030-01-01T00:00:01Z',
    completedAtUtc: null,
    steps: [{
      id: 'step-1',
      sequence: 1,
      agentName: 'Planning Agent',
      responsibility: 'Create a structured plan.',
      allowedTools: [],
      status: 'Completed',
      retryCount: 0,
      error: null,
      startedAtUtc: '2030-01-01T00:00:00Z',
      completedAtUtc: '2030-01-01T00:00:01Z',
      durationMilliseconds: 10,
      toolCalls: [],
    }],
    validationResults: [{
      ruleName: 'revision-0:ToolAllowList',
      passed: true,
      message: 'Every observed tool was allow-listed.',
      createdAtUtc: '2030-01-01T00:00:01Z',
    }],
    shortlist: [{
      jobId: 'job-1',
      companyName: 'Northstar Labs',
      title: 'Backend Engineer',
      location: 'Colombo',
      employmentType: 'FullTime',
      workMode: 'Hybrid',
      score: 92.5,
      rank: 1,
      reasonSummary: 'Skills and title match.',
      isApproved: false,
    }],
    approvalDecisions: [],
  }],
};

describe('AgentWorkflowsPage', () => {
  beforeEach(() => {
    vi.mocked(useSWR).mockReturnValue({
      data: workflowList,
      error: undefined,
      isLoading: false,
      mutate: vi.fn(),
    } as never);
  });

  it('shows validation evidence and requires explicit human approval', () => {
    render(<AgentWorkflowsPage />);

    expect(screen.getByRole('heading', { name: /Backend Engineer/ })).toBeInTheDocument();
    expect(screen.getByText(/ToolAllowList/)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Approve shortlist' })).toBeEnabled();
    expect(screen.getByRole('button', { name: 'Reject' })).toBeEnabled();
  });
});
