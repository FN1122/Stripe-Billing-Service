import React, { useEffect, useState } from 'react';
import { Row, Col, Container } from 'react-bootstrap';
import { DollarSign, CreditCard, Users, TrendingUp } from 'lucide-react';
import { MetricCard } from '../../components/common/MetricCard';
import { RevenueChart } from '../../components/common/RevenueChart';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { dashboardApi } from '../../api/dashboardApi';
import { DashboardStats, ActivityFeedItem } from '../../types/dashboard';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const DashboardPage: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [revenueData, setRevenueData] = useState<any[]>([]);
  const [activityFeed, setActivityFeed] = useState<ActivityFeedItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        const [statsRes, revenueRes, activityRes] = await Promise.all([
          dashboardApi.getStats(),
          dashboardApi.getRevenueChart(30),
          dashboardApi.getActivityFeed(10),
        ]);

        if (statsRes.data.isValid) setStats(statsRes.data.data);
        if (revenueRes.data.isValid) setRevenueData(revenueRes.data.data || []);
        if (activityRes.data.isValid) {
          const items = (activityRes.data.data || []).map((item: ActivityFeedItem, idx: number) => ({
            ...item,
            id: item.id || `${item.timestamp}-${idx}`,
          }));
          setActivityFeed(items);
        }
      } catch (err) {
        console.error('Failed to fetch dashboard data:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  const activityColumns: DataTableColumn<ActivityFeedItem>[] = [
    {
      key: 'type',
      label: 'Type',
      width: '100px',
      render: (value) => <span style={{ textTransform: 'capitalize' }}>{value}</span>,
    },
    { key: 'description', label: 'Description' },
    {
      key: 'amount',
      label: 'Amount',
      render: (value, row) => value ? formatCurrency(value, row.currency) : '-',
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <span className="badge bg-success">{value}</span>,
    },
    {
      key: 'timestamp',
      label: 'Time',
      render: (value) => formatDate(value, 'MMM dd, HH:mm'),
    },
  ];

  return (
    <>
      <Container fluid>
        {isLoading ? (
          <div style={{ padding: '20px' }}>
            <LoadingSkeleton count={5} height={100} />
          </div>
        ) : (
          <>
            {/* Metrics */}
            <Row className="mb-4">
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Total Revenue"
                  value={formatCurrency(stats?.totalRevenue || 0)}
                  change={stats?.revenueChange}
                  icon={<DollarSign size={24} />}
                  trend={stats?.revenueChange || 0 >= 0 ? 'up' : 'down'}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="MRR"
                  value={formatCurrency(stats?.mrrCurrent || 0)}
                  icon={<TrendingUp size={24} />}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Active Subscriptions"
                  value={stats?.activeSubscriptions || 0}
                  change={stats?.subscriptionChange}
                  icon={<CreditCard size={24} />}
                  trend={stats?.subscriptionChange || 0 >= 0 ? 'up' : 'down'}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Total Customers"
                  value={stats?.totalCustomers || 0}
                  change={stats?.customerChange}
                  icon={<Users size={24} />}
                  trend={stats?.customerChange || 0 >= 0 ? 'up' : 'down'}
                />
              </Col>
            </Row>

            {/* Charts */}
            <Row className="mb-4">
              <Col lg={12}>
                <div style={{
                  background: 'white',
                  padding: '20px',
                  borderRadius: '8px',
                  border: '1px solid #e2e8f0',
                }}>
                  {revenueData.length > 0 ? (
                    <RevenueChart data={revenueData} title="30-Day Revenue Trend" />
                  ) : (
                    <p style={{ textAlign: 'center', color: '#718096' }}>No revenue data</p>
                  )}
                </div>
              </Col>
            </Row>

            {/* Activity */}
            <Row>
              <Col lg={12}>
                <div style={{
                  background: 'white',
                  padding: '20px',
                  borderRadius: '8px',
                  border: '1px solid #e2e8f0',
                }}>
                  <h4 style={{ marginBottom: '20px' }}>Recent Activity</h4>
                  <DataTable<ActivityFeedItem>
                    columns={activityColumns}
                    data={activityFeed}
                    rowKey="id"
                  />
                </div>
              </Col>
            </Row>
          </>
        )}
      </Container>
    </>
  );
};

export default DashboardPage;
