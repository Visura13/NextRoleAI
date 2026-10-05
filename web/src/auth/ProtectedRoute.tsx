import { Navigate, Outlet, useLocation } from 'react-router';
import type { UserRole } from '../api/types';
import { LoadingState } from '../components/States';
import { useAuth } from './useAuth';

export function ProtectedRoute({ role }: { role?: UserRole }) {
  const { session, isReady } = useAuth();
  const location = useLocation();

  if (!isReady) return <LoadingState label="Checking your session" />;
  if (!session) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (role && session.user.role !== role) return <Navigate to="/dashboard" replace />;
  return <Outlet />;
}
