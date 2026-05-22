import React, { useState } from 'react';
import { Row, Col, Card, Button, Form, Badge, Spinner } from 'react-bootstrap';
import { exportApi } from '../../api/exportApi';
import { Download, FileText, Users, CreditCard, FileSpreadsheet, Receipt, History } from 'lucide-react';

interface ExportOption {
  id: string;
  label: string;
  description: string;
  icon: React.ReactNode;
  exportFn: (format: string, dateRange?: { from: string; to: string }) => Promise<any>;
}

export const ExportCenterPage: React.FC = () => {
  const [isExporting, setIsExporting] = useState<string | null>(null);
  const [format, setFormat] = useState('csv');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const exportOptions: ExportOption[] = [
    {
      id: 'transactions',
      label: 'Transactions',
      description: 'Export all payment transactions',
      icon: <CreditCard size={24} className="text-primary" />,
      exportFn: (fmt) => exportApi.exportTransactions({ format: fmt, from: dateFrom || undefined, to: dateTo || undefined }),
    },
    {
      id: 'invoices',
      label: 'Invoices',
      description: 'Export invoice records',
      icon: <FileText size={24} className="text-success" />,
      exportFn: (fmt) => exportApi.exportInvoices({ format: fmt, from: dateFrom || undefined, to: dateTo || undefined }),
    },
    {
      id: 'customers',
      label: 'Customers',
      description: 'Export customer data',
      icon: <Users size={24} className="text-info" />,
      exportFn: (fmt) => exportApi.exportCustomers({ format: fmt }),
    },
    {
      id: 'subscriptions',
      label: 'Subscriptions',
      description: 'Export subscription records',
      icon: <FileSpreadsheet size={24} className="text-warning" />,
      exportFn: (fmt) => exportApi.exportSubscriptions({ format: fmt }),
    },
    {
      id: 'refunds',
      label: 'Refunds',
      description: 'Export refund records',
      icon: <Receipt size={24} className="text-danger" />,
      exportFn: (fmt) => exportApi.exportRefunds({ format: fmt, from: dateFrom || undefined, to: dateTo || undefined }),
    },
    {
      id: 'audit-logs',
      label: 'Audit Logs',
      description: 'Export audit trail',
      icon: <History size={24} className="text-secondary" />,
      exportFn: (fmt) => exportApi.exportAuditLog({ format: fmt, from: dateFrom || undefined, to: dateTo || undefined }),
    },
  ];

  const handleExport = async (option: ExportOption) => {
    try {
      setIsExporting(option.id);
      const response = await option.exportFn(format);
      const blob = new Blob([response.data], { type: format === 'csv' ? 'text/csv' : 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${option.id}_export_${new Date().toISOString().split('T')[0]}.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error(`Failed to export ${option.id}:`, err);
    } finally {
      setIsExporting(null);
    }
  };

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Export Center</h2>

        {/* Filters */}
        <Card className="border-0 shadow-sm mb-4">
          <Card.Body>
            <h6 className="mb-3">Export Settings</h6>
            <Row>
              <Col md={3}>
                <Form.Group>
                  <Form.Label>Format</Form.Label>
                  <Form.Select value={format} onChange={(e) => setFormat(e.target.value)}>
                    <option value="csv">CSV</option>
                    <option value="pdf">PDF</option>
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={3}>
                <Form.Group>
                  <Form.Label>From Date</Form.Label>
                  <Form.Control type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
                </Form.Group>
              </Col>
              <Col md={3}>
                <Form.Group>
                  <Form.Label>To Date</Form.Label>
                  <Form.Control type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
                </Form.Group>
              </Col>
            </Row>
          </Card.Body>
        </Card>

        {/* Export Options Grid */}
        <Row>
          {exportOptions.map((option) => (
            <Col md={4} key={option.id} className="mb-4">
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="d-flex flex-column">
                  <div className="d-flex align-items-center gap-3 mb-3">
                    {option.icon}
                    <div>
                      <h6 className="mb-0">{option.label}</h6>
                      <small className="text-muted">{option.description}</small>
                    </div>
                  </div>
                  <div className="mt-auto">
                    <Button
                      variant="outline-primary"
                      className="w-100"
                      onClick={() => handleExport(option)}
                      disabled={isExporting === option.id}
                    >
                      {isExporting === option.id ? (
                        <Spinner animation="border" size="sm" className="me-1" />
                      ) : (
                        <Download size={16} className="me-1" />
                      )}
                      Export {option.label}
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      </div>
    </>
  );
};

export default ExportCenterPage;
