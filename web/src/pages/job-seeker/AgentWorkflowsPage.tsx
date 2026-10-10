import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { AgentWorkflow, AgentWorkflowList } from '../../api/types';
import { EmptyState, ErrorState, LoadingState, Notice } from '../../components/States';

const defaultObjective = 'Find the best 3 software engineering jobs for my confirmed profile.';

export function AgentWorkflowsPage() {
  const { data, error, isLoading, mutate } = useSWR<AgentWorkflowList>('/api/agent-workflows?pageSize=10', swrFetcher, { shouldRetryOnError: false });
  const [objective, setObjective] = useState(defaultObjective);
  const [selected, setSelected] = useState<AgentWorkflow | null>(null);
  const [notice, setNotice] = useState<{ kind: 'success' | 'error' | 'info'; message: string } | null>(null);
  const [busy, setBusy] = useState(false);
  const activeWorkflow = selected ?? data?.items[0] ?? null;

  async function start(event: FormEvent) {
    event.preventDefault();
    setNotice(null);
    setBusy(true);
    try {
      const workflow = await api.request<AgentWorkflow>('/api/agent-workflows', {
        method: 'POST',
        body: JSON.stringify({ objective: objective.trim() }),
      }, true);
      setSelected(workflow);
      await mutate();
      setNotice({
        kind: workflow.status === 'Failed' ? 'error' : 'info',
        message: workflow.status === 'Failed' ? workflow.failureMessage ?? 'The workflow failed safely.' : 'Your agentic job search is ready.',
      });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The workflow could not be started.' });
    } finally {
      setBusy(false);
    }
  }

  if (isLoading) return <LoadingState label="Loading agent workflows" />;
  if (error) return <ErrorState message="Agent workflows could not be loaded." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Agentic job search</p><h1>Delegate the search. Discover your best-fit roles.</h1><p>Specialized agents use your confirmed profile to find and rank relevant opportunities.</p></header>
      {notice && <Notice kind={notice.kind}>{notice.message}</Notice>}
      <form className="surface-form" onSubmit={start}>
        <label>Job-search objective<textarea minLength={20} maxLength={500} rows={3} value={objective} onChange={(event) => setObjective(event.target.value)} /></label>
        <small>Describe the type and number of roles you want. Objectives are treated as data and checked for instruction injection.</small>
        <div className="form-actions"><button className="button" disabled={busy || objective.trim().length < 20} type="submit">{busy ? 'Running agents…' : 'Start controlled workflow'}</button></div>
      </form>

      {!data?.items.length && !activeWorkflow ? <EmptyState title="No workflow history" message="Start with a confirmed CV to create your first auditable shortlist." /> : (
        <div className="workflow-layout">
          <aside className="workflow-history"><h2>Run history</h2>{data?.items.map((workflow) => <button className={activeWorkflow?.id === workflow.id ? 'workflow-history__item is-active' : 'workflow-history__item'} key={workflow.id} onClick={() => setSelected(workflow)} type="button"><span>{workflow.objective}</span><small>{new Date(workflow.createdAtUtc).toLocaleString()}</small></button>)}</aside>
          {activeWorkflow && <WorkflowDetails workflow={activeWorkflow} />}
        </div>
      )}
    </div>
  );
}

function WorkflowDetails({ workflow }: { workflow: AgentWorkflow }) {
  return <section className="workflow-details">
    {workflow.failureMessage && <Notice kind="error">{workflow.failureCode}: {workflow.failureMessage}</Notice>}
    <h3>Agentic search results</h3>
    {!workflow.shortlist.length ? <p className="muted">No matching jobs were found by this run.</p> : <div className="agent-shortlist">{workflow.shortlist.map((item) => <article key={item.jobId}><span className="recommendation__score"><strong>{item.score}</strong><small>score</small></span><div><h4>#{item.rank} {item.title}</h4><p>{item.companyName} · {item.location} · {item.workMode}</p><small>{item.reasonSummary}</small></div></article>)}</div>}
  </section>;
}
