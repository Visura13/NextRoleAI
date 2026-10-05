import { Navigate } from 'react-router';
import { useAuth } from '../../auth/useAuth';

export function DashboardRedirect() {
  const { session } = useAuth();
  return <Navigate replace to={session?.user.role === 'Recruiter' ? '/recruiter' : '/job-seeker'} />;
}
