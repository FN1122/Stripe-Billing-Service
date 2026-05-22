import React, { useEffect, useState } from 'react';
import { Row, Col, Card } from 'react-bootstrap';
import { Building2, Users, CreditCard, DollarSign, AlertTriangle, Activity } from 'lucide-react';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { superAdminApi, SystemDashboard, TenantSummary } from '../../api/superAdminApi';
import { StatusBadge } from '../../components/common/StatusBadge';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const SuperAdminDashboardPage: React.FC = () => {
  const [dashboard, setDashboard] = useState<SystemDashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const res = await superAdminApi.getDashboard();
        if (res.data.isValid) {
          setDashboard(res.data.data);
        }
      } catch (err) {
        console.error('Failed to fetch dashboard:', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (isLoading) {
    return (
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Platform Dashboard</h2>
        <LoadingSkeleton count={6} height={100} />
      </div>
    );
  }

  const stats = [
    { label: 'Total Tenants', value: dashboard?.totalTenants ?? 0, icon: Building2, color: '#6366f1' },
    { label: 'Active Tenants', value: dashboard?.activeTenants ?? 0, icon: Activity, color: '#22c55e' },
    { label: 'Total Customers', value: dashboard?.totalCustomers ?? 0, icon: Users, color: '#3b82f6' },
    { label: 'Active Subscriptions', value: dashboard?.activeSubscriptions ?? 0, icon: CreditCard, color: '#f59e0b' },
    { label: 'Total Revenue', value: formatCurrency(dashboard?.totalRevenue ?? 0), icon: DollarSign, color: '#10b981' },
    { label: 'Failed Payments (30d)', value: dashboard?.failedPaymentsLast30Days ?? 0, icon: AlertTriangle, color: '#ef4444' },
  ];

  return (
    <div style={{ padding: '20px' }}>
      <h2 style={{ marginBottom: '20px' }}>Platform Dashboard</h2>

      <Row className="g-3 mb-4">
        {stats.map((stat, idx) => {
          const Icon = stat.icon;
          return (
            <Col key={idx} md={4} lg={2}>
              <Card style={{ border: '1px solid #e2e8f0', borderRadius: '8px' }}>
                <Card.Body style={{ padding: '16px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
                    <div style={{ background: `${stat.color}15`, borderRadius: '8px', padding: '8px', display: 'flex' }}>
                      <Icon size={18} color={stat.color} />
                    </div>
                  </div>
                  <div style={{ fontSize: '24px', fontWeight: 700 }}>{stat.value}</div>
                  <div style={{ fontSize: '13px', color: '#64748b' }}>{stat.label}</div>
                </Card.Body>
              </Card>
            </Col>
          );
        })}
      </Row>

      <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <h5 style={{ marginBottom: '16px' }}>Recent Tenants</h5>
        <table className="table table-hover mb-0">
          <thead>
            <tr>
              <th>Name</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {(dashboard?.recentTenants || []).map((tenant: TenantSummary) => (
              <tr key={tenant.id}>
                <td>{tenant.name}</td>
                <td><StatusBadge status={tenant.isActive ? 'Active' : 'Inactive'} /></td>
                <td>{formatDate(tenant.createdAt)}</td>
              </tr>
            ))}
            {(!dashboard?.recentTenants || dashboard.recentTenants.length === 0) && (
              <tr><td colSpan={3} style={{ textAlign: 'center', color: '#94a3b8' }}>No tenants found</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default SuperAdminDashboardPage;
