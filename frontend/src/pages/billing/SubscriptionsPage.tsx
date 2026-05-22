import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { SubscriptionBadge } from '../../components/common/SubscriptionBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { subscriptionApi } from '../../api/subscriptionApi';
import { Subscription } from '../../types/subscription';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const SubscriptionsPage: React.FC = () => {
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchSubscriptions = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (status) filters.status = status;
        if (search) filters.search = search;

        const res = await subscriptionApi.getSubscriptions(page, pageSize, filters);
        if (res.data.isValid) {
          setSubscriptions(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch subscriptions:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchSubscriptions();
  }, [page, search, status]);

  const columns: DataTableColumn<Subscription>[] = [
    { key: 'customerName', label: 'Customer' },
    { key: 'planName', label: 'Plan' },
    {
      key: 'planAmount',
      label: 'Amount',
      render: (value) => formatCurrency(value),
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <SubscriptionBadge status={value} />,
    },
    {
      key: 'currentPeriodEnd',
      label: 'Renews',
      render: (value) => formatDate(value),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Subscriptions</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search customers..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="active">Active</option>
              <option value="trialing">Trialing</option>
              <option value="past_due">Past Due</option>
              <option value="cancelled">Cancelled</option>
            </Form.Select>
          </Col>
          <Col md={3} className="text-end">
            <Button variant="primary">New Subscription</Button>
          </Col>
        </Row>

        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '8px',
          border: '1px solid #e2e8f0',
        }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<Subscription>
              columns={columns}
              data={subscriptions}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default SubscriptionsPage;
