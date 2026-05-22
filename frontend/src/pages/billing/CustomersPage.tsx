import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { customerApi } from '../../api/customerApi';
import { Customer } from '../../types/customer';
import { formatCurrency } from '../../utils/formatters';

export const CustomersPage: React.FC = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchCustomers = async () => {
      try {
        setIsLoading(true);
        const res = await customerApi.getCustomers(page, pageSize, search);
        if (res.data.isValid) {
          setCustomers(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch customers:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchCustomers();
  }, [page, search]);

  const columns: DataTableColumn<Customer>[] = [
    { key: 'name', label: 'Name' },
    { key: 'email', label: 'Email' },
    {
      key: 'subscriptionCount',
      label: 'Subscriptions',
      render: (value) => <span className="badge bg-info">{value}</span>,
    },
    {
      key: 'totalSpent',
      label: 'Total Spent',
      render: (value) => formatCurrency(value),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Customers</h2>

        <Row className="mb-4">
          <Col md={9}>
            <SearchInput placeholder="Search by name or email..." onSearch={setSearch} />
          </Col>
          <Col md={3} className="text-end">
            <Button variant="primary">Add Customer</Button>
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
            <DataTable<Customer>
              columns={columns}
              data={customers}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default CustomersPage;
