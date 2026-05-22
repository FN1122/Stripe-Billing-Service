import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
}

const ROLE_HIERARCHY: Record<string, number> = {
  SuperAdmin: 100,
  Admin: 50,
  Manager: 20,
  Viewer: 10,
};

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRole,
}) => {
  const { isAuthenticated, isLoading, user, isImpersonating } = useAuth();

  if (isLoading) {
    return (
      <div style={{ padding: '20px' }}>
        <LoadingSkeleton count={5} />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // SuperAdmin NOT impersonating: can only access SuperAdmin-specific routes
  if (user?.role === 'SuperAdmin' && !isImpersonating) {
    if (requiredRole === 'SuperAdmin') {
      return <>{children}</>;
    }
    // Block access to tenant-scoped routes — redirect to SuperAdmin dashboard
    return <Navigate to="/super-admin/dashboard" replace />;
  }

  // When impersonating, user.role is already set to 'Viewer' by the auth context
  // So role hierarchy check below handles it naturally

  // Role hierarchy check for non-SuperAdmin users
  if (requiredRole) {
    const userLevel = ROLE_HIERARCHY[user?.role || ''] || 0;
    const requiredLevel = ROLE_HIERARCHY[requiredRole] || 0;
    if (userLevel < requiredLevel) {
      return <Navigate to="/" replace />;
    }
  }

  return <>{children}</>;
};
