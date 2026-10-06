import type { ApplicationStatus } from '../api/types';
import { readableLabel } from './jobFormatting';

export function ApplicationStatusBadge({ status }: { status: ApplicationStatus }) {
  return (
    <span className={`application-status application-status--${status.toLowerCase()}`}>
      {readableLabel(status)}
    </span>
  );
}
