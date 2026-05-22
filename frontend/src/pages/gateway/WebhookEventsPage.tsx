import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Badge, Modal, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { webhookEventApi } from '../../api/webhookEventApi';
import { WebhookEvent } from '../../types/webhookEvent';
import { Webhook, RefreshCw, Eye, AlertCircle, CheckCircle, Clock } from 'lucide-react';

export const WebhookEventsPage: React.FC = () => {
  const [events, setEvents] = useState<WebhookEvent[]>([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [selectedEvent, setSelectedEvent] = useState<WebhookEvent | null>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);

  useEffect(() => {
    fetchEvents();
  }, [search, statusFilter]);

  const fetchEvents = async () => {
    try {
      setIsLoading(true);
      const res = await webhookEventApi.getInboundEvents({ page: 1, pageSize: 50, search, status: statusFilter || undefined });
      if (res.data.isValid) setEvents(res.data.data || []);
    } catch (err) {
      console.error('Failed to fetch webhook events:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRetry = async (id: string) => {
    try {
      await webhookEventApi.replayEvent(id);
      fetchEvents();
    } catch (err) {
      console.error('Failed to retry event:', err);
    }
  };

  const handleViewDetail = (event: WebhookEvent) => {
    setSelectedEvent(event);
    setShowDetailModal(true);
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'processed':
      case 'delivered': return <CheckCircle size={14} className="text-success" />;
      case 'failed': return <AlertCircle size={14} className="text-danger" />;
      default: return <Clock size={14} className="text-warning" />;
    }
  };

  const columns: DataTableColumn<WebhookEvent>[] = [
    {
      key: 'eventType',
      label: 'Event Type',
      render: (value) => <code className="bg-light px-2 py-1 rounded" style={{ fontSize: '0.8rem' }}>{value}</code>,
    },
    {
      key: 'stripeEventId',
      label: 'Stripe Event ID',
      render: (value) => <span style={{ fontSize: '0.8rem' }}>{value?.substring(0, 20)}...</span>,
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => (
        <Badge bg={value === 'delivered' || value === 'processed' ? 'success' : value === 'failed' ? 'danger' : 'warning'} className="d-inline-flex align-items-center gap-1">
          {getStatusIcon(value)} {value}
        </Badge>
      ),
    },
    {
      key: 'retryCount',
      label: 'Retries',
      render: (value) => value ?? 0,
    },
    {
      key: 'createdAt',
      label: 'Received',
      render: (value) => new Date(value).toLocaleString(),
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_, row) => (
        <div className="d-flex gap-1">
          <Button size="sm" variant="outline-primary" onClick={() => handleViewDetail(row)} title="View">
            <Eye size={14} />
          </Button>
          {row.status === 'failed' && (
            <Button size="sm" variant="outline-warning" onClick={() => handleRetry(row.id)} title="Retry">
              <RefreshCw size={14} />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Webhook Event Log</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search by event type..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="processed">Processed</option>
              <option value="failed">Failed</option>
              <option value="pending">Pending</option>
            </Form.Select>
          </Col>
          <Col md={3} className="text-end">
            <Button variant="outline-secondary" onClick={fetchEvents}>
              <RefreshCw size={16} className="me-1" /> Refresh
            </Button>
          </Col>
        </Row>

        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<WebhookEvent>
              columns={columns}
              data={events}
              rowKey="id"
            />
          )}
        </div>
      </div>

      {/* Detail Modal */}
      <Modal show={showDetailModal} onHide={() => setShowDetailModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Webhook Event Detail</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedEvent && (
            <div>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Event Type:</strong>
                  <div><code>{selectedEvent.eventType}</code></div>
                </Col>
                <Col md={6}>
                  <strong>Status:</strong>
                  <div>
                    <Badge bg={selectedEvent.status === 'delivered' || selectedEvent.status === 'processed' ? 'success' : 'danger'}>
                      {selectedEvent.status}
                    </Badge>
                  </div>
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Stripe Event ID:</strong>
                  <div><code style={{ fontSize: '0.8rem' }}>{selectedEvent.stripeEventId}</code></div>
                </Col>
                <Col md={6}>
                  <strong>Received:</strong>
                  <div>{new Date(selectedEvent.createdAt).toLocaleString()}</div>
                </Col>
              </Row>
              {selectedEvent.payload && (
                <div>
                  <strong>Payload:</strong>
                  <pre className="bg-light p-3 rounded mt-1" style={{ maxHeight: '300px', overflow: 'auto', fontSize: '0.8rem' }}>
                    {JSON.stringify(JSON.parse(selectedEvent.payload), null, 2)}
                  </pre>
                </div>
              )}
            </div>
          )}
        </Modal.Body>
      </Modal>
    </>
  );
};

export default WebhookEventsPage;
