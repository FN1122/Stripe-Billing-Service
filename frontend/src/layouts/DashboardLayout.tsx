import React, { Suspense } from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/layout/Sidebar';
import { Topbar } from '../components/layout/Topbar';
import { ImpersonationBanner } from '../components/layout/ImpersonationBanner';
import { useSidebar } from '../hooks/useSidebar';
import { useAuth } from '../hooks/useAuth';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import './DashboardLayout.scss';

export const DashboardLayout: React.FC = () => {
  const { isCollapsed } = useSidebar();
  const { isImpersonating } = useAuth();

  return (
    <div className="dashboard-layout">
      <ImpersonationBanner />
      <div style={{ paddingTop: isImpersonating ? '40px' : '0', width: '100%' }}>
        <Sidebar />
        <div className={`dashboard-content ${isCollapsed ? 'sidebar-collapsed' : ''}`}>
          <Topbar />
          <main className="dashboard-main">
            <Suspense fallback={<div style={{ padding: '20px' }}><LoadingSkeleton count={5} /></div>}>
              <Outlet />
            </Suspense>
          </main>
        </div>
      </div>
    </div>
  );
};
