import { Link } from 'react-router';
import { EmptyState } from '../../components/States';

export function ComingSoonPage() {
  return (
    <EmptyState
      title="Applications arrive in Part 8"
      message="The screen is reserved, but submitting and reviewing applications will only be enabled after the shared workflow, audit trail, and notifications are implemented."
      action={<Link className="button button--secondary" to="/jobs">Explore jobs</Link>}
    />
  );
}
