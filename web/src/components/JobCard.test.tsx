import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { describe, expect, it } from 'vitest';
import type { Job } from '../api/types';
import { JobCard } from './JobCard';

const job: Job = {
  id: '2c737b8e-f835-4ce6-8d0a-dbc91a392a19',
  companyId: 'd8d2ea7d-ae6d-4326-9ee0-dd5a6a96eae2',
  companyName: 'Northstar Labs',
  title: 'Platform Engineer',
  description: 'Build dependable services used by teams around the world.',
  location: 'Colombo',
  employmentType: 'FullTime',
  workMode: 'Hybrid',
  minimumYearsExperience: 2,
  salaryMinimum: 250000,
  salaryMaximum: 400000,
  salaryCurrency: 'LKR',
  status: 'Published',
  publishedAtUtc: '2026-10-05T10:00:00Z',
  closesAtUtc: null,
  skills: [{ name: 'C#', isRequired: true }, { name: 'PostgreSQL', isRequired: false }],
  createdAtUtc: '2026-10-05T09:00:00Z',
  updatedAtUtc: '2026-10-05T10:00:00Z',
};

describe('JobCard', () => {
  it('renders structured job details and a detail link', () => {
    render(<MemoryRouter><JobCard job={job} /></MemoryRouter>);

    expect(screen.getByRole('heading', { name: 'Platform Engineer' })).toBeInTheDocument();
    expect(screen.getByText('Northstar Labs')).toBeInTheDocument();
    expect(screen.getByText('Full time')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /view role/i })).toHaveAttribute('href', `/jobs/${job.id}`);
  });
});
