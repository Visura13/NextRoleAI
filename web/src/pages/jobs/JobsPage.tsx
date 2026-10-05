import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import type { EmploymentType, Job, PagedResult, WorkMode } from '../../api/types';
import { publicFetcher } from '../../api/client';
import { JobCard } from '../../components/JobCard';
import { EmptyState, ErrorState, LoadingState } from '../../components/States';

interface Filters {
  search: string;
  location: string;
  employmentType: '' | EmploymentType;
  workMode: '' | WorkMode;
  sortBy: string;
}

const initialFilters: Filters = { search: '', location: '', employmentType: '', workMode: '', sortBy: 'newest' };

function buildPath(filters: Filters, page: number) {
  const params = new URLSearchParams({ page: String(page), pageSize: '9', sortBy: filters.sortBy });
  Object.entries(filters).forEach(([key, value]) => {
    if (key !== 'sortBy' && value) params.set(key, value);
  });
  return `/api/jobs?${params}`;
}

export function JobsPage() {
  const [draft, setDraft] = useState(initialFilters);
  const [filters, setFilters] = useState(initialFilters);
  const [page, setPage] = useState(1);
  const path = buildPath(filters, page);
  const { data, error, isLoading } = useSWR<PagedResult<Job>>(path, publicFetcher);

  function submit(event: FormEvent) {
    event.preventDefault();
    setPage(1);
    setFilters(draft);
  }

  function clearFilters() {
    setDraft(initialFilters);
    setFilters(initialFilters);
    setPage(1);
  }

  return (
    <div className="container page-stack">
      <header className="page-hero page-hero--jobs">
        <p className="eyebrow">Opportunity, without the noise</p>
        <h1>Explore roles with the context that matters.</h1>
        <p>Search every published opportunity, or open Recommendations to rank this catalog against your confirmed CV.</p>
      </header>
      <form className="filter-panel" onSubmit={submit}>
        <label>Keywords<input placeholder="Title, company, or skill" value={draft.search} onChange={(event) => setDraft({ ...draft, search: event.target.value })} /></label>
        <label>Location<input placeholder="City or region" value={draft.location} onChange={(event) => setDraft({ ...draft, location: event.target.value })} /></label>
        <label>Employment type<select value={draft.employmentType} onChange={(event) => setDraft({ ...draft, employmentType: event.target.value as Filters['employmentType'] })}><option value="">Any type</option><option value="FullTime">Full time</option><option value="PartTime">Part time</option><option value="Contract">Contract</option><option value="Internship">Internship</option></select></label>
        <label>Work mode<select value={draft.workMode} onChange={(event) => setDraft({ ...draft, workMode: event.target.value as Filters['workMode'] })}><option value="">Any mode</option><option value="OnSite">On-site</option><option value="Hybrid">Hybrid</option><option value="Remote">Remote</option></select></label>
        <label>Sort by<select value={draft.sortBy} onChange={(event) => setDraft({ ...draft, sortBy: event.target.value })}><option value="newest">Newest</option><option value="title">Title</option><option value="closingDate">Closing soon</option></select></label>
        <button className="button" type="submit">Search jobs</button>
      </form>
      <div className="results-heading">
        <div><p className="eyebrow">Open roles</p><h2>{data ? `${data.totalCount} opportunities` : 'Finding opportunities'}</h2></div>
        {(filters.search || filters.location || filters.employmentType || filters.workMode) && <button className="text-button" type="button" onClick={clearFilters}>Clear filters</button>}
      </div>
      {isLoading && <LoadingState label="Finding open roles" />}
      {error && <ErrorState message="Jobs could not be loaded. Check that the API is running and try again." />}
      {data?.items.length === 0 && <EmptyState title="No roles match yet" message="Try widening your location or removing a filter." action={<button className="button button--secondary" type="button" onClick={clearFilters}>Reset search</button>} />}
      {data && data.items.length > 0 && <div className="job-grid">{data.items.map((job) => <JobCard job={job} key={job.id} />)}</div>}
      {data && data.totalPages > 1 && (
        <nav className="pagination" aria-label="Job results pages">
          <button disabled={page === 1} type="button" onClick={() => setPage((current) => current - 1)}>Previous</button>
          <span>Page {page} of {data.totalPages}</span>
          <button disabled={page === data.totalPages} type="button" onClick={() => setPage((current) => current + 1)}>Next</button>
        </nav>
      )}
    </div>
  );
}
