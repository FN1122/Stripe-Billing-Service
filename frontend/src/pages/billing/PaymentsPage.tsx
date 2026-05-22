import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { paymentApi } from '../../api/paymentApi';
import { PaymentTransaction } from '../../types/payment';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const PaymentsPage: React.FC = () => {
  const [payments, setPayments] = useState<PaymentTransaction[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchPayments = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (status) filters.status = status;
        if (search) filters.search = search;

        const res = await paymentApi.getTransactions(page, pageSize, filters);
        if (res.data.isValid) {
          setPayments(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch payments:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPayments();
  }, [page, search, status]);

  const columns: DataTableColumn<PaymentTransaction>[] = [
    { key: 'customerName', label: 'Customer' },
    {
      key: 'amount',
      label: 'Amount',
      render: (value, row) => formatCurrency(value, row.currency),
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <StatusBadge status={value} />,
    },
    {
      key: 'paymentMethod',
      label: 'Method',
      render: (value, row) => `${row.paymentMethodBrand} ****${row.paymentMethodLast4}`,
    },
    {
      key: 'createdAt',
      label: 'Date',
      render: (value) => formatDate(value),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Payments</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search by customer name or email..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="succeeded">Succeeded</option>
              <option value="pending">Pending</option>
              <option value="failed">Failed</option>
            </Form.Select>
          </Col>
          <Col md={3} className="text-end">
            <Button variant="primary">Export</Button>
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
            <DataTable<PaymentTransaction>
              columns={columns}
              data={payments}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default PaymentsPage;
