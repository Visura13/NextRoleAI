import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { AgentDecisionType, AgentWorkflow, AgentWorkflowList } from '../../api/types';
import { EmptyState, ErrorState, LoadingState, Notice } from '../../components/States';

const defaultObjective = 'Find the best 3 software engineering jobs for my confirmed profile.';

export function AgentWorkflowsPage() {
  const { data, error, isLoading, mutate } = useSWR<AgentWorkflowList>('/api/agent-workflows?pageSize=10', swrFetcher, { shouldRetryOnError: false });
  const [objective, setObjective] = useState(defaultObjective);
  const [selected, setSelected] = useState<AgentWorkflow | null>(null);
  const [feedback, setFeedback] = useState('');
  const [revisedObjective, setRevisedObjective] = useState(defaultObjective);
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
      setRevisedObjective(workflow.objective);
      await mutate();
      setNotice({
        kind: workflow.status === 'Failed' ? 'error' : 'info',
        message: workflow.status === 'Failed' ? workflow.failureMessage ?? 'The workflow failed safely.' : 'The proposal is ready for review. Nothing is published until you approve it.',
      });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The workflow could not be started.' });
    } finally {
      setBusy(false);
    }
  }

  async function decide(decision: AgentDecisionType) {
    if (!activeWorkflow) return;
    setBusy(true);
    setNotice(null);
    try {
      const workflow = await api.request<AgentWorkflow>(`/api/agent-workflows/${activeWorkflow.id}/decision`, {
        method: 'POST',
        body: JSON.stringify({
          decision,
          feedback: feedback.trim(),
          revisedObjective: decision === 'RequestRevision' ? revisedObjective.trim() : null,
        }),
      }, true);
      setSelected(workflow);
      setFeedback('');
      await mutate();
      setNotice({ kind: 'success', message: workflow.finalSummary ?? 'Your decision was recorded.' });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The decision could not be recorded.' });
    } finally {
      setBusy(false);
    }
  }

  if (isLoading) return <LoadingState label="Loading agent workflows" />;
  if (error) return <ErrorState message="Agent workflows could not be loaded." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Controlled Agentic AI</p><h1>Delegate a shortlist, keep the final say.</h1><p>Four specialized agents plan, inspect your confirmed profile, discover ranked roles, and validate the proposal. Publication always pauses for your approval.</p></header>
      {notice && <Notice kind={notice.kind}>{notice.message}</Notice>}
      <form className="surface-form" onSubmit={start}>
        <label>Job-search objective<textarea minLength={20} maxLength={500} rows={3} value={objective} onChange={(event) => setObjective(event.target.value)} /></label>
        <small>Describe the type and number of roles you want. Objectives are treated as data and checked for instruction injection.</small>
        <div className="form-actions"><button className="button" disabled={busy || objective.trim().length < 20} type="submit">{busy ? 'Running agents…' : 'Start controlled workflow'}</button></div>
      </form>

      {!data?.items.length && !activeWorkflow ? <EmptyState title="No workflow history" message="Start with a confirmed CV to create your first auditable shortlist." /> : (
        <div className="workflow-layout">
          <aside className="workflow-history"><h2>Run history</h2>{data?.items.map((workflow) => <button className={activeWorkflow?.id === workflow.id ? 'workflow-history__item is-active' : 'workflow-history__item'} key={workflow.id} onClick={() => { setSelected(workflow); setRevisedObjective(workflow.objective); }} type="button"><strong>{workflow.status}</strong><span>{workflow.objective}</span><small>{new Date(workflow.createdAtUtc).toLocaleString()}</small></button>)}</aside>
          {activeWorkflow && <WorkflowDetails workflow={activeWorkflow} busy={busy} feedback={feedback} revisedObjective={revisedObjective} onFeedback={setFeedback} onRevisedObjective={setRevisedObjective} onDecision={decide} />}
        </div>
      )}
    </div>
  );
}

function WorkflowDetails({ workflow, busy, feedback, revisedObjective, onFeedback, onRevisedObjective, onDecision }: { workflow: AgentWorkflow; busy: boolean; feedback: string; revisedObjective: string; onFeedback: (value: string) => void; onRevisedObjective: (value: string) => void; onDecision: (decision: AgentDecisionType) => void }) {
  return <section className="workflow-details">
    <div className="cv-file-summary"><div><p className="eyebrow">Run {workflow.id.slice(0, 8)}</p><h2>{workflow.status}</h2><p>{workflow.finalSummary ?? workflow.objective}</p></div><span className={`status status--${workflow.status === 'Completed' ? 'published' : workflow.status === 'Failed' || workflow.status === 'Rejected' ? 'closed' : 'draft'}`}>{workflow.approvalStatus}</span></div>
    {workflow.failureMessage && <Notice kind="error">{workflow.failureCode}: {workflow.failureMessage}</Notice>}
    <h3>Proposed shortlist</h3>
    {!workflow.shortlist.length ? <p className="muted">No jobs were published by this run.</p> : <div className="agent-shortlist">{workflow.shortlist.map((item) => <article key={item.jobId}><span className="recommendation__score"><strong>{item.score}</strong><small>score</small></span><div><h4>#{item.rank} {item.title}</h4><p>{item.companyName} · {item.location} · {item.workMode}</p><small>{item.reasonSummary}</small></div><span className={`status status--${item.isApproved ? 'published' : 'draft'}`}>{item.isApproved ? 'Published' : 'Proposed'}</span></article>)}</div>}
    <h3>Deterministic validation</h3>
    <div className="validation-grid">{workflow.validationResults.map((result) => <div className={result.passed ? 'validation-card is-passed' : 'validation-card is-failed'} key={`${result.ruleName}-${result.createdAtUtc}`}><strong>{result.passed ? 'PASS' : 'FAIL'} · {result.ruleName}</strong><p>{result.message}</p></div>)}</div>
    <h3>Execution history</h3>
    <ol className="agent-timeline">{workflow.steps.map((step) => <li key={step.id}><div><strong>{step.agentName}</strong><span>{step.status} · {step.durationMilliseconds ?? 0} ms · retries {step.retryCount}</span></div><p>{step.responsibility}</p>{step.allowedTools.length > 0 && <small>Allowed tools: {step.allowedTools.join(', ')}</small>}{step.toolCalls.map((call) => <div className="tool-call" key={call.id}><code>{call.toolName}</code><span>{call.succeeded ? 'Succeeded' : 'Failed'} · {call.durationMilliseconds} ms</span></div>)}</li>)}</ol>
    {workflow.status === 'PendingApproval' && <div className="approval-panel"><h3>Human approval required</h3><p>Approval publishes only these validated jobs to your shortlist. Rejection publishes nothing. Revision reruns every agent and safety check.</p><label>Decision note<textarea maxLength={1000} rows={2} value={feedback} onChange={(event) => onFeedback(event.target.value)} /></label><label>Revised objective<textarea minLength={20} maxLength={500} rows={3} value={revisedObjective} onChange={(event) => onRevisedObjective(event.target.value)} /></label><div className="button-row"><button className="button" disabled={busy} onClick={() => onDecision('Approve')} type="button">Approve shortlist</button><button className="button button--secondary" disabled={busy || revisedObjective.trim().length < 20} onClick={() => onDecision('RequestRevision')} type="button">Request revision</button><button className="button button--danger" disabled={busy} onClick={() => onDecision('Reject')} type="button">Reject</button></div></div>}
  </section>;
}
