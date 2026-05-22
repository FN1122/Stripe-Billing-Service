import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { RefundBadge } from '../../components/common/RefundBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { refundApi } from '../../api/refundApi';
import { Refund } from '../../types/refund';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const RefundsPage: React.FC = () => {
  const [refunds, setRefunds] = useState<Refund[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchRefunds = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (status) filters.status = status;
        if (search) filters.search = search;

        const res = await refundApi.getRefunds(page, pageSize, filters);
        if (res.data.isValid) {
          setRefunds(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch refunds:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRefunds();
  }, [page, search, status]);

  const columns: DataTableColumn<Refund>[] = [
    { key: 'customerName', label: 'Customer' },
    {
      key: 'amount',
      label: 'Amount',
      render: (value, row) => formatCurrency(value, row.currency),
    },
    { key: 'reason', label: 'Reason' },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <RefundBadge status={value} />,
    },
    {
      key: 'createdAt',
      label: 'Requested',
      render: (value) => formatDate(value),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Refunds</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search customers..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="pending">Pending</option>
              <option value="processing">Processing</option>
              <option value="completed">Completed</option>
              <option value="failed">Failed</option>
            </Form.Select>
          </Col>
          <Col md={3} className="text-end">
            <Button variant="primary">Request Refund</Button>
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
            <DataTable<Refund>
              columns={columns}
              data={refunds}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default RefundsPage;
