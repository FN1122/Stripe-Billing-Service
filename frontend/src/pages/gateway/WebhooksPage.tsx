import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { webhookApi } from '../../api/webhookApi';
import { WebhookSubscription } from '../../types/webhook';
import { formatDate } from '../../utils/formatters';

export const WebhooksPage: React.FC = () => {
  const [webhooks, setWebhooks] = useState<WebhookSubscription[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchWebhooks = async () => {
      try {
        setIsLoading(true);
        const res = await webhookApi.getSubscriptions(page, pageSize);
        if (res.data.isValid) {
          setWebhooks(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch webhooks:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchWebhooks();
  }, [page]);

  const columns: DataTableColumn<WebhookSubscription>[] = [
    { key: 'description', label: 'Description' },
    { key: 'webhookUrl', label: 'URL', render: (v) => <code style={{ fontSize: '11px' }}>{v}</code> },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'Active' : 'Inactive'} />,
    },
    {
      key: 'createdAt',
      label: 'Created',
      render: (value) => formatDate(value),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h2>Webhooks</h2>
          <Button variant="primary">Add Webhook</Button>
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
            <DataTable<WebhookSubscription>
              columns={columns}
              data={webhooks}
              rowKey="id"
            />
          )}
        </div>
      </div>
    </>
  );
};

export default WebhooksPage;
