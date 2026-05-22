import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Form, Button, Badge } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { usageApi } from '../../api/usageApi';
import { UsageRecord, UsageRecordResponse, UsageDashboard } from '../../types/usage';
import { Activity, BarChart3, Zap, TrendingUp } from 'lucide-react';

export const UsageBillingPage: React.FC = () => {
  const [dashboard, setDashboard] = useState<UsageDashboard | null>(null);
  const [records, setRecords] = useState<UsageRecord[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [dashRes, recordsRes] = await Promise.all([
        usageApi.getDashboard(),
        usageApi.getUsageRecords({ page: 1, pageSize: 20 }),
      ]);
      if (dashRes.data.isValid) {
        setDashboard(dashRes.data.data);
      }
      if (recordsRes.data.isValid) {
        setRecords(recordsRes.data.data || []);
      }
    } catch (err) {
      console.error('Failed to fetch usage data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const getDailyUsageData = () => {
    if (!dashboard?.usageTrend) return [];
    return Object.entries(dashboard.usageTrend)
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([date, quantity]) => ({ date, quantity }));
  };

  const columns: DataTableColumn<UsageRecord>[] = [
    {
      key: 'subscriptionId',
      label: 'Subscription',
      render: (value) => <code className="bg-light px-2 py-1 rounded" style={{ fontSize: '0.8rem' }}>{value?.substring(0, 8)}...</code>,
    },
    {
      key: 'quantity',
      label: 'Quantity',
      render: (value) => <span className="fw-medium">{value?.toLocaleString()}</span>,
    },
    {
      key: 'action',
      label: 'Action',
      render: (value) => (
        <Badge bg={value === 'increment' ? 'info' : 'warning'}>{value}</Badge>
      ),
    },
    {
      key: 'timestamp',
      label: 'Timestamp',
      render: (value) => new Date(value).toLocaleString(),
    },
    {
      key: 'idempotencyKey',
      label: 'Idempotency Key',
      render: (value) => value ? <code style={{ fontSize: '0.75rem' }}>{value}</code> : <span className="text-muted">-</span>,
    },
  ];

  const dailyUsage = getDailyUsageData();

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Usage-Based Billing</h2>

        {/* Dashboard Metrics */}
        {isLoading ? (
          <LoadingSkeleton count={4} height={100} />
        ) : dashboard && (
          <>
            <Row className="mb-4">
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Activity size={24} className="text-primary mb-2" />
                    <h6 className="text-muted">Current Period Usage</h6>
                    <h3>{dashboard.totalUsageCurrentPeriod?.toLocaleString()}</h3>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Zap size={24} className="text-warning mb-2" />
                    <h6 className="text-muted">Estimated Revenue</h6>
                    <h3>${dashboard.estimatedRevenue?.toLocaleString()}</h3>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <BarChart3 size={24} className="text-success mb-2" />
                    <h6 className="text-muted">Active Meters</h6>
                    <h3>{dashboard.activeMeteredSubscriptions}</h3>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <TrendingUp size={24} className="text-info mb-2" />
                    <h6 className="text-muted">Top Consumers</h6>
                    <h3>{dashboard.topConsumers?.length || 0}</h3>
                  </Card.Body>
                </Card>
              </Col>
            </Row>

            {/* Top Consumers */}
            {dashboard.topConsumers && dashboard.topConsumers.length > 0 && (
              <Card className="border-0 shadow-sm mb-4">
                <Card.Body>
                  <h6 className="mb-3">Top Consumers</h6>
                  <div className="d-flex gap-3 flex-wrap">
                    {dashboard.topConsumers.map((consumer, idx) => (
                      <div key={idx} className="d-flex align-items-center gap-2 bg-light rounded px-3 py-2">
                        <Badge bg="primary" pill>{idx + 1}</Badge>
                        <div>
                          <div className="fw-medium" style={{ fontSize: '0.9rem' }}>{consumer.customerName || consumer.customerId?.substring(0, 12) + '...'}</div>
                          <small className="text-muted">{consumer.totalUsage?.toLocaleString()} units</small>
                        </div>
                      </div>
                    ))}
                  </div>
                </Card.Body>
              </Card>
            )}

            {/* Usage Trend Chart */}
            {dailyUsage.length > 0 && (
              <Card className="border-0 shadow-sm mb-4">
                <Card.Body>
                  <h6 className="mb-3">Usage Trend</h6>
                  <div className="d-flex align-items-end gap-1" style={{ height: '150px' }}>
                    {dailyUsage.map((day, idx) => {
                      const maxVal = Math.max(...dailyUsage.map(d => d.quantity), 1);
                      return (
                        <div
                          key={idx}
                          className="flex-fill"
                          style={{
                            background: '#4f46e5',
                            height: `${(day.quantity / maxVal) * 100}%`,
                            minHeight: '2px',
                            borderRadius: '2px 2px 0 0',
                            opacity: 0.7,
                          }}
                          title={`${day.date}: ${day.quantity}`}
                        />
                      );
                    })}
                  </div>
                </Card.Body>
              </Card>
            )}
          </>
        )}

        {/* Usage Records */}
        <h5 className="mb-3">Recent Usage Records</h5>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<UsageRecord>
              columns={columns}
              data={records}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default UsageBillingPage;
