import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { userApi } from '../../api/userApi';
import { User } from '../../types/auth';
import { formatDate } from '../../utils/formatters';

export const UsersPage: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (search) filters.search = search;

        const res = await userApi.getUsers(page, pageSize, filters);
        if (res.data.isValid) {
          setUsers(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch users:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchUsers();
  }, [page, search]);

  const columns: DataTableColumn<User>[] = [
    { key: 'fullName', label: 'Name' },
    { key: 'email', label: 'Email' },
    {
      key: 'role',
      label: 'Role',
      render: (value) => <span className="badge bg-primary">{value}</span>,
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'Active' : 'Inactive'} />,
    },
    {
      key: 'lastLoginAt',
      label: 'Last Login',
      render: (value) => value ? formatDate(value) : 'Never',
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h2>Users</h2>
          <Button variant="primary">Add User</Button>
        </div>

        <Row className="mb-4">
          <Col md={12}>
            <SearchInput placeholder="Search by name or email..." onSearch={setSearch} />
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
            <DataTable<User>
              columns={columns}
              data={users}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default UsersPage;
