import React, { useEffect, useState } from 'react';
import { Row, Col, Card } from 'react-bootstrap';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { superAdminApi, TenantRevenueBreakdown } from '../../api/superAdminApi';
import { formatCurrency } from '../../utils/formatters';

export const GlobalAnalyticsPage: React.FC = () => {
  const [breakdown, setBreakdown] = useState<TenantRevenueBreakdown[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await superAdminApi.getTenantBreakdown();
        if (res.data.isValid) {
          setBreakdown(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch analytics:', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  if (isLoading) {
    return (
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Platform Analytics</h2>
        <LoadingSkeleton count={5} height={60} />
      </div>
    );
  }

  const totalRevenue = breakdown.reduce((sum, t) => sum + t.totalRevenue, 0);
  const totalSubs = breakdown.reduce((sum, t) => sum + t.activeSubscriptions, 0);
  const totalCustomers = breakdown.reduce((sum, t) => sum + t.totalCustomers, 0);

  return (
    <div style={{ padding: '20px' }}>
      <h2 style={{ marginBottom: '20px' }}>Platform Analytics</h2>

      <Row className="g-3 mb-4">
        <Col md={4}>
          <Card style={{ border: '1px solid #e2e8f0', borderRadius: '8px' }}>
            <Card.Body>
              <div style={{ fontSize: '13px', color: '#64748b' }}>Total Platform Revenue</div>
              <div style={{ fontSize: '28px', fontWeight: 700, color: '#10b981' }}>{formatCurrency(totalRevenue)}</div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card style={{ border: '1px solid #e2e8f0', borderRadius: '8px' }}>
            <Card.Body>
              <div style={{ fontSize: '13px', color: '#64748b' }}>Total Active Subscriptions</div>
              <div style={{ fontSize: '28px', fontWeight: 700, color: '#3b82f6' }}>{totalSubs}</div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card style={{ border: '1px solid #e2e8f0', borderRadius: '8px' }}>
            <Card.Body>
              <div style={{ fontSize: '13px', color: '#64748b' }}>Total Customers</div>
              <div style={{ fontSize: '28px', fontWeight: 700, color: '#6366f1' }}>{totalCustomers}</div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <h5 style={{ marginBottom: '16px' }}>Revenue Breakdown by Tenant</h5>
        <table className="table table-hover mb-0">
          <thead>
            <tr>
              <th>Tenant</th>
              <th style={{ textAlign: 'right' }}>Revenue</th>
              <th style={{ textAlign: 'right' }}>Active Subs</th>
              <th style={{ textAlign: 'right' }}>Customers</th>
            </tr>
          </thead>
          <tbody>
            {breakdown.map((t) => (
              <tr key={t.tenantId}>
                <td>{t.tenantName}</td>
                <td style={{ textAlign: 'right' }}>{formatCurrency(t.totalRevenue)}</td>
                <td style={{ textAlign: 'right' }}>{t.activeSubscriptions}</td>
                <td style={{ textAlign: 'right' }}>{t.totalCustomers}</td>
              </tr>
            ))}
            {breakdown.length === 0 && (
              <tr><td colSpan={4} style={{ textAlign: 'center', color: '#94a3b8' }}>No data available</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default GlobalAnalyticsPage;
