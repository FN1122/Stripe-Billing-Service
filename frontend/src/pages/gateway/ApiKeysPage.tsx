import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { apiKeyApi } from '../../api/apiKeyApi';
import { ApiKey } from '../../types/apiKey';
import { formatDate } from '../../utils/formatters';

export const ApiKeysPage: React.FC = () => {
  const [keys, setKeys] = useState<ApiKey[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchKeys = async () => {
      try {
        setIsLoading(true);
        const res = await apiKeyApi.getKeys(page, pageSize);
        if (res.data.isValid) {
          setKeys(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch API keys:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchKeys();
  }, [page]);

  const columns: DataTableColumn<ApiKey>[] = [
    { key: 'name', label: 'Name' },
    { key: 'environment', label: 'Environment' },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'Active' : 'Inactive'} />,
    },
    {
      key: 'lastUsedAt',
      label: 'Last Used',
      render: (value) => value ? formatDate(value) : 'Never',
    },
    { key: 'totalRequests', label: 'Requests' },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h2>API Keys</h2>
          <Button variant="primary">Create API Key</Button>
        </div>

        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '8px',
          border: '1px solid #e2e8f0',
        }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<ApiKey>
              columns={columns}
              data={keys}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default ApiKeysPage;
