import React, { useEffect, useState } from 'react';
import { Row, Col, Form } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { logApi } from '../../api/logApi';
import { LogEntry } from '../../types/log';
import { formatDate } from '../../utils/formatters';

export const LogsPage: React.FC = () => {
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (status) filters.status = status;
        if (search) filters.search = search;

        const res = await logApi.getLogs(page, pageSize, filters);
        if (res.data.isValid) {
          setLogs(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch logs:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchLogs();
  }, [page, search, status]);

  const columns: DataTableColumn<LogEntry>[] = [
    { key: 'endpoint', label: 'Endpoint' },
    { key: 'method', label: 'Method' },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <StatusBadge status={value} />,
    },
    {
      key: 'responseStatusCode',
      label: 'HTTP',
      render: (value) => <span className="badge bg-info">{value}</span>,
    },
    {
      key: 'durationMs',
      label: 'Duration',
      render: (value) => `${value}ms`,
    },
    {
      key: 'createdAt',
      label: 'Time',
      render: (value) => formatDate(value, 'MMM dd, HH:mm:ss'),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>API Logs</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search by endpoint..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="success">Success</option>
              <option value="error">Error</option>
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
            <DataTable<LogEntry>
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

export default LogsPage;
