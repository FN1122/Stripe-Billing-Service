import React, { useEffect, useState } from 'react';
import { Row, Col, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { auditApi } from '../../api/auditApi';
import { AuditLogEntry } from '../../types/audit';
import { formatDate } from '../../utils/formatters';

export const AuditLogPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLogEntry[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [action, setAction] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (action) filters.action = action;
        if (search) filters.search = search;

        const res = await auditApi.getLogs(page, pageSize, filters);
        if (res.data.isValid) {
          setLogs(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch audit logs:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchLogs();
  }, [page, search, action]);

  const columns: DataTableColumn<AuditLogEntry>[] = [
    { key: 'userEmail', label: 'User' },
    { key: 'action', label: 'Action' },
    { key: 'entityType', label: 'Entity Type' },
    { key: 'tenantName', label: 'Tenant' },
    {
      key: 'createdAt',
      label: 'Timestamp',
      render: (value) => formatDate(value, 'MMM dd, HH:mm:ss'),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Audit Logs</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search by user email..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={action} onChange={(e) => setAction(e.target.value)}>
              <option value="">All Actions</option>
              <option value="create">Create</option>
              <option value="update">Update</option>
              <option value="delete">Delete</option>
            </Form.Select>
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
            <DataTable<AuditLogEntry>
              columns={columns}
              data={logs}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default AuditLogPage;
