import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Form } from 'react-bootstrap';
import { Download } from 'lucide-react';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { InvoiceViewer } from '../../components/common/InvoiceViewer';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { invoiceApi } from '../../api/invoiceApi';
import { Invoice } from '../../types/invoice';
import { formatCurrency, formatDate } from '../../utils/formatters';

export const InvoicesPage: React.FC = () => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchInvoices = async () => {
      try {
        setIsLoading(true);
        const filters: any = {};
        if (status) filters.status = status;
        if (search) filters.search = search;

        const res = await invoiceApi.getInvoices(page, pageSize, filters);
        if (res.data.isValid) {
          setInvoices(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch invoices:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchInvoices();
  }, [page, search, status]);

  const columns: DataTableColumn<Invoice>[] = [
    { key: 'invoiceNumber', label: 'Invoice #' },
    { key: 'customerName', label: 'Customer' },
    {
      key: 'total',
      label: 'Amount',
      render: (value, row) => formatCurrency(value, row.currency),
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <StatusBadge status={value} />,
    },
    {
      key: 'createdAt',
      label: 'Date',
      render: (value) => formatDate(value),
    },
    {
      key: 'invoicePdfUrl',
      label: 'Action',
      render: (value) => (
        <Button
          variant="sm"
          size="sm"
          href={value}
          target="_blank"
          className="btn-sm"
        >
          <Download size={14} />
        </Button>
      ),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Invoices</h2>

        <Row className="mb-4">
          <Col md={6}>
            <SearchInput placeholder="Search invoices..." onSearch={setSearch} />
          </Col>
          <Col md={3}>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="paid">Paid</option>
              <option value="draft">Draft</option>
              <option value="open">Open</option>
            </Form.Select>
          </Col>
          <Col md={3} className="text-end">
            <Button variant="primary">Create Invoice</Button>
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
            <DataTable<Invoice>
              columns={columns}
              data={invoices}
              rowKey="id"
              onRowClick={setSelectedInvoice}
            />
          )}
        </div>

        <InvoiceViewer
          isOpen={!!selectedInvoice}
          invoice={selectedInvoice}
          onClose={() => setSelectedInvoice(null)}
        />
      </div>
    </>
  );
};

export default InvoicesPage;
