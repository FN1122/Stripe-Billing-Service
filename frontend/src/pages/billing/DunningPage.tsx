import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Badge, Modal, Form } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { StatusBadge } from '../../components/common/StatusBadge';
import { dunningApi } from '../../api/dunningApi';
import { DunningScheduleResponse, DunningDashboard } from '../../types/dunning';
import { AlertTriangle, Play, Pause, XCircle, BarChart3, Clock, CheckCircle, TrendingUp } from 'lucide-react';

export const DunningPage: React.FC = () => {
  const [dashboard, setDashboard] = useState<DunningDashboard | null>(null);
  const [schedules, setSchedules] = useState<DunningScheduleResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [dashRes, schedulesRes] = await Promise.all([
        dunningApi.getDashboard(),
        dunningApi.getSchedules({ page: 1, pageSize: 50 }),
      ]);
      if (dashRes.data.isValid) setDashboard(dashRes.data.data);
      if (schedulesRes.data.isValid) setSchedules(schedulesRes.data.data || []);
    } catch (err) {
      console.error('Failed to fetch dunning data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handlePause = async (id: string) => {
    try {
      await dunningApi.pauseSchedule(id);
      fetchData();
    } catch (err) {
      console.error('Failed to pause schedule:', err);
    }
  };

  const handleResume = async (id: string) => {
    try {
      await dunningApi.resumeSchedule(id);
      fetchData();
    } catch (err) {
      console.error('Failed to resume schedule:', err);
    }
  };

  const handleCancel = async (id: string) => {
    try {
      await dunningApi.cancelSchedule(id);
      fetchData();
    } catch (err) {
      console.error('Failed to cancel schedule:', err);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'warning';
      case 'paused': return 'secondary';
      case 'completed': return 'success';
      case 'cancelled': return 'danger';
      default: return 'info';
    }
  };

  const columns: DataTableColumn<DunningScheduleResponse>[] = [
    {
      key: 'stripeInvoiceId',
      label: 'Invoice',
      render: (value) => <code className="bg-light px-2 py-1 rounded" style={{ fontSize: '0.8rem' }}>{value?.substring(0, 12)}...</code>,
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <Badge bg={getStatusColor(value)}>{value}</Badge>,
    },
    {
      key: 'currentStep',
      label: 'Step',
      render: (value, row) => (
        <span>{value + 1} / {row.maxSteps}</span>
      ),
    },
    {
      key: 'nextRetryAt',
      label: 'Next Retry',
      render: (value) => value ? new Date(value).toLocaleString() : <span className="text-muted">-</span>,
    },
    {
      key: 'totalRetryAttempts',
      label: 'Retries',
    },
    {
      key: 'originalFailureDate',
      label: 'Started',
      render: (value) => new Date(value).toLocaleDateString(),
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_, row) => (
        <div className="d-flex gap-1">
          {row.status === 'active' && (
            <Button size="sm" variant="outline-warning" onClick={() => handlePause(row.id)} title="Pause">
              <Pause size={14} />
            </Button>
          )}
          {row.status === 'paused' && (
            <Button size="sm" variant="outline-success" onClick={() => handleResume(row.id)} title="Resume">
              <Play size={14} />
            </Button>
          )}
          {(row.status === 'active' || row.status === 'paused') && (
            <Button size="sm" variant="outline-danger" onClick={() => handleCancel(row.id)} title="Cancel">
              <XCircle size={14} />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Dunning Management</h2>

        {/* Dashboard */}
        {isLoading ? (
          <LoadingSkeleton count={4} height={100} />
        ) : dashboard && (
          <Row className="mb-4">
            <Col md={3}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <AlertTriangle size={24} className="text-warning mb-2" />
                  <h6 className="text-muted">Active Schedules</h6>
                  <h3>{dashboard.activeDunningCount}</h3>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <CheckCircle size={24} className="text-success mb-2" />
                  <h6 className="text-muted">Recovered</h6>
                  <h3>{dashboard.recoveredCount}</h3>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <XCircle size={24} className="text-danger mb-2" />
                  <h6 className="text-muted">Failed</h6>
                  <h3>{dashboard.lostCount}</h3>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <TrendingUp size={24} className="text-info mb-2" />
                  <h6 className="text-muted">Recovery Rate</h6>
                  <h3>{dashboard.recoveryRate ? `${(dashboard.recoveryRate * 100).toFixed(1)}%` : '0%'}</h3>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        )}

        {/* Schedules Table */}
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<DunningScheduleResponse>
              columns={columns}
              data={schedules}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default DunningPage;
