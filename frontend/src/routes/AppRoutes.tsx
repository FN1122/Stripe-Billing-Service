import React, { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { DashboardLayout } from '../layouts/DashboardLayout';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';

const LoginPage = lazy(() => import('../pages/auth/LoginPage').then(m => ({ default: m.LoginPage })));
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage').then(m => ({ default: m.DashboardPage })));
const PaymentsPage = lazy(() => import('../pages/billing/PaymentsPage').then(m => ({ default: m.PaymentsPage })));
const SubscriptionsPage = lazy(() => import('../pages/billing/SubscriptionsPage').then(m => ({ default: m.SubscriptionsPage })));
const CustomersPage = lazy(() => import('../pages/billing/CustomersPage').then(m => ({ default: m.CustomersPage })));
const InvoicesPage = lazy(() => import('../pages/billing/InvoicesPage').then(m => ({ default: m.InvoicesPage })));
const RefundsPage = lazy(() => import('../pages/billing/RefundsPage').then(m => ({ default: m.RefundsPage })));
const PlansPage = lazy(() => import('../pages/billing/PlansPage').then(m => ({ default: m.PlansPage })));
const RevenueAnalyticsPage = lazy(() => import('../pages/analytics/RevenueAnalyticsPage').then(m => ({ default: m.RevenueAnalyticsPage })));
const ApiKeysPage = lazy(() => import('../pages/gateway/ApiKeysPage').then(m => ({ default: m.ApiKeysPage })));
const LogsPage = lazy(() => import('../pages/gateway/LogsPage').then(m => ({ default: m.LogsPage })));
const WebhooksPage = lazy(() => import('../pages/gateway/WebhooksPage').then(m => ({ default: m.WebhooksPage })));
const UsersPage = lazy(() => import('../pages/users/UsersPage').then(m => ({ default: m.UsersPage })));
const SettingsPage = lazy(() => import('../pages/settings/SettingsPage').then(m => ({ default: m.SettingsPage })));
const AuditLogPage = lazy(() => import('../pages/audit/AuditLogPage').then(m => ({ default: m.AuditLogPage })));
const CouponsPage = lazy(() => import('../pages/billing/CouponsPage').then(m => ({ default: m.CouponsPage })));
const UsageBillingPage = lazy(() => import('../pages/billing/UsageBillingPage').then(m => ({ default: m.UsageBillingPage })));
const TaxConfigPage = lazy(() => import('../pages/billing/TaxConfigPage').then(m => ({ default: m.TaxConfigPage })));
const DunningPage = lazy(() => import('../pages/billing/DunningPage').then(m => ({ default: m.DunningPage })));
const CreditsPage = lazy(() => import('../pages/billing/CreditsPage').then(m => ({ default: m.CreditsPage })));
const EmailTemplatesPage = lazy(() => import('../pages/billing/EmailTemplatesPage').then(m => ({ default: m.EmailTemplatesPage })));
const ExportCenterPage = lazy(() => import('../pages/billing/ExportCenterPage').then(m => ({ default: m.ExportCenterPage })));
const ConnectPage = lazy(() => import('../pages/billing/ConnectPage').then(m => ({ default: m.ConnectPage })));
const WebhookEventsPage = lazy(() => import('../pages/gateway/WebhookEventsPage').then(m => ({ default: m.WebhookEventsPage })));

// SuperAdmin pages
const TenantsPage = lazy(() => import('../pages/superadmin/TenantsPage').then(m => ({ default: m.TenantsPage })));
const SuperAdminDashboardPage = lazy(() => import('../pages/superadmin/SuperAdminDashboardPage').then(m => ({ default: m.SuperAdminDashboardPage })));
const GlobalAnalyticsPage = lazy(() => import('../pages/superadmin/GlobalAnalyticsPage').then(m => ({ default: m.GlobalAnalyticsPage })));
const GlobalEmailTemplatesPage = lazy(() => import('../pages/superadmin/GlobalEmailTemplatesPage').then(m => ({ default: m.GlobalEmailTemplatesPage })));
const PlatformSettingsPage = lazy(() => import('../pages/superadmin/PlatformSettingsPage').then(m => ({ default: m.PlatformSettingsPage })));

const SuspenseFallback = () => <div style={{ padding: '20px' }}><LoadingSkeleton count={5} /></div>;

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<Suspense fallback={<SuspenseFallback />}><LoginPage /></Suspense>} />

        {/* SuperAdmin-specific routes */}
        <Route element={<ProtectedRoute requiredRole="SuperAdmin"><DashboardLayout /></ProtectedRoute>}>
          <Route path="super-admin/dashboard" element={<SuperAdminDashboardPage />} />
          <Route path="super-admin/tenants" element={<TenantsPage />} />
          <Route path="super-admin/analytics" element={<GlobalAnalyticsPage />} />
          <Route path="super-admin/email-templates" element={<GlobalEmailTemplatesPage />} />
          <Route path="super-admin/settings" element={<PlatformSettingsPage />} />
          <Route path="super-admin/audit" element={<AuditLogPage />} />
        </Route>

        {/* Tenant-scoped routes (Admin, Manager, Viewer, and impersonating SuperAdmin) */}
        <Route element={<ProtectedRoute><DashboardLayout /></ProtectedRoute>}>
          <Route index element={<DashboardPage />} />
          <Route path="payments" element={<PaymentsPage />} />
          <Route path="subscriptions" element={<SubscriptionsPage />} />
          <Route path="customers" element={<CustomersPage />} />
          <Route path="invoices" element={<InvoicesPage />} />
          <Route path="refunds" element={<RefundsPage />} />
          <Route path="plans" element={<PlansPage />} />
          <Route path="analytics" element={<RevenueAnalyticsPage />} />
          <Route path="api-keys" element={<ApiKeysPage />} />
          <Route path="logs" element={<LogsPage />} />
          <Route path="webhooks" element={<WebhooksPage />} />
          <Route path="webhook-events" element={<WebhookEventsPage />} />
          <Route path="coupons" element={<CouponsPage />} />
          <Route path="usage-billing" element={<UsageBillingPage />} />
          <Route path="credits" element={<CreditsPage />} />
          <Route path="dunning" element={<DunningPage />} />
          <Route path="exports" element={<ExportCenterPage />} />
          <Route path="connect" element={<ProtectedRoute requiredRole="Admin"><ConnectPage /></ProtectedRoute>} />
          <Route path="users" element={<ProtectedRoute requiredRole="Admin"><UsersPage /></ProtectedRoute>} />
          <Route path="settings" element={<ProtectedRoute requiredRole="Admin"><SettingsPage /></ProtectedRoute>} />
          <Route path="audit-logs" element={<ProtectedRoute requiredRole="Admin"><AuditLogPage /></ProtectedRoute>} />
          <Route path="tax-config" element={<ProtectedRoute requiredRole="Admin"><TaxConfigPage /></ProtectedRoute>} />
          <Route path="email-templates" element={<ProtectedRoute requiredRole="Admin"><EmailTemplatesPage /></ProtectedRoute>} />
        </Route>

        {/* Legacy redirect for old /tenants path */}
        <Route path="tenants" element={<Navigate to="/super-admin/tenants" replace />} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
