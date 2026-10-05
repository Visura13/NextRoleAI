import { Link } from 'react-router';
import { EmptyState } from '../../components/States';

export function NotFoundPage() {
  return (
    <div className="container narrow-page">
      <EmptyState title="This page is off the map" message="The address may have changed or the page may no longer exist." action={<Link className="button" to="/">Return home</Link>} />
    </div>
  );
}
