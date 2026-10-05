import type { ReactNode } from 'react';

export function LoadingState({ label = 'Loading' }: { label?: string }) {
  return (
    <div className="state-panel" role="status" aria-live="polite">
      <span className="spinner" aria-hidden="true" />
      <p>{label}…</p>
    </div>
  );
}

export function ErrorState({ message, action }: { message: string; action?: ReactNode }) {
  return (
    <div className="state-panel state-panel--error" role="alert">
      <p>{message}</p>
      {action}
    </div>
  );
}

export function EmptyState({ title, message, action }: { title: string; message: string; action?: ReactNode }) {
  return (
    <div className="empty-state">
      <span className="empty-state__mark" aria-hidden="true">N</span>
      <h2>{title}</h2>
      <p>{message}</p>
      {action}
    </div>
  );
}

export function FieldError({ message }: { message?: string }) {
  return message ? <span className="field-error">{message}</span> : null;
}

export function Notice({ kind, children }: { kind: 'success' | 'error' | 'info'; children: ReactNode }) {
  return <div className={`notice notice--${kind}`} role={kind === 'error' ? 'alert' : 'status'}>{children}</div>;
}
