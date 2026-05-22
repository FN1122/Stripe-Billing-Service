import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Modal, Form, Badge, Tab, Tabs } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { StatusBadge } from '../../components/common/StatusBadge';
import { emailApi } from '../../api/emailApi';
import { EmailTemplateResponse, EmailLogResponse } from '../../types/email';
import { Mail, Plus, Eye, Edit2, Send, Clock } from 'lucide-react';

export const EmailTemplatesPage: React.FC = () => {
  const [templates, setTemplates] = useState<EmailTemplateResponse[]>([]);
  const [logs, setLogs] = useState<EmailLogResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('templates');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showPreviewModal, setShowPreviewModal] = useState(false);
  const [previewHtml, setPreviewHtml] = useState('');
  const [templateForm, setTemplateForm] = useState({
    name: '',
    subject: '',
    htmlBody: '',
    textBody: '',
    category: 'billing',
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [templatesRes, logsRes] = await Promise.all([
        emailApi.getTemplates(),
        emailApi.getLogs({ page: 1, pageSize: 50 }),
      ]);
      if (templatesRes.data.isValid) setTemplates(templatesRes.data.data || []);
      if (logsRes.data.isValid) setLogs(logsRes.data.data || []);
    } catch (err) {
      console.error('Failed to fetch email data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await emailApi.createTemplate(templateForm);
      setShowCreateModal(false);
      setTemplateForm({ name: '', subject: '', htmlBody: '', textBody: '', category: 'billing' });
      fetchData();
    } catch (err) {
      console.error('Failed to create template:', err);
    }
  };

  const handlePreview = async (templateId: string) => {
    try {
      const res = await emailApi.previewTemplate({ templateId, variables: {} });
      if (res.data.isValid) {
        setPreviewHtml(res.data.data || '');
        setShowPreviewModal(true);
      }
    } catch (err) {
      console.error('Failed to preview template:', err);
    }
  };

  const templateColumns: DataTableColumn<EmailTemplateResponse>[] = [
    {
      key: 'templateKey',
      label: 'Template',
      render: (value, row) => (
        <div>
          <div className="fw-medium">{value}</div>
          <small className="text-muted">{row.subject}</small>
        </div>
      ),
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'active' : 'inactive'} />,
    },
    {
      key: 'updatedAt',
      label: 'Last Updated',
      render: (value) => value ? new Date(value).toLocaleDateString() : '-',
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_, row) => (
        <div className="d-flex gap-1">
          <Button size="sm" variant="outline-primary" onClick={() => handlePreview(row.id)} title="Preview">
            <Eye size={14} />
          </Button>
          <Button size="sm" variant="outline-secondary" title="Edit">
            <Edit2 size={14} />
          </Button>
        </div>
      ),
    },
  ];

  const logColumns: DataTableColumn<EmailLogResponse>[] = [
    {
      key: 'to',
      label: 'Recipient',
      render: (value) => <span className="fw-medium">{value}</span>,
    },
    {
      key: 'subject',
      label: 'Subject',
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => (
        <Badge bg={value === 'sent' ? 'success' : value === 'failed' ? 'danger' : 'warning'}>
          {value === 'sent' ? <Send size={12} className="me-1" /> : <Clock size={12} className="me-1" />}
          {value}
        </Badge>
      ),
    },
    {
      key: 'sentAt',
      label: 'Sent At',
      render: (value) => value ? new Date(value).toLocaleString() : '-',
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Email & Notifications</h2>

        <Tabs activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'templates')} className="mb-4">
          <Tab eventKey="templates" title={`Templates (${templates.length})`}>
            <Row className="mb-4">
              <Col md={8}></Col>
              <Col md={4} className="text-end">
                <Button variant="primary" onClick={() => setShowCreateModal(true)}>
                  <Plus size={16} className="me-1" /> Create Template
                </Button>
              </Col>
            </Row>

            <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              {isLoading ? <LoadingSkeleton count={5} height={50} /> : (
                <DataTable<EmailTemplateResponse> columns={templateColumns} data={templates} rowKey="id" />
              )}
            </div>
          </Tab>

          <Tab eventKey="logs" title={`Email Logs (${logs.length})`}>
            <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0', marginTop: '16px' }}>
              {isLoading ? <LoadingSkeleton count={5} height={50} /> : (
                <DataTable<EmailLogResponse> columns={logColumns} data={logs} rowKey="id" />
              )}
            </div>
          </Tab>
        </Tabs>
      </div>

      {/* Create Template Modal */}
      <Modal show={showCreateModal} onHide={() => setShowCreateModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Create Email Template</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Row>
              <Col md={8}>
                <Form.Group className="mb-3">
                  <Form.Label>Template Name</Form.Label>
                  <Form.Control value={templateForm.name} onChange={(e) => setTemplateForm({ ...templateForm, name: e.target.value })} placeholder="e.g. payment_success" />
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Category</Form.Label>
                  <Form.Select value={templateForm.category} onChange={(e) => setTemplateForm({ ...templateForm, category: e.target.value })}>
                    <option value="billing">Billing</option>
                    <option value="subscription">Subscription</option>
                    <option value="dunning">Dunning</option>
                    <option value="notification">Notification</option>
                  </Form.Select>
                </Form.Group>
              </Col>
            </Row>
            <Form.Group className="mb-3">
              <Form.Label>Subject Line</Form.Label>
              <Form.Control value={templateForm.subject} onChange={(e) => setTemplateForm({ ...templateForm, subject: e.target.value })} placeholder="e.g. Your payment of {{amount}} was successful" />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>HTML Body</Form.Label>
              <Form.Control as="textarea" rows={6} value={templateForm.htmlBody} onChange={(e) => setTemplateForm({ ...templateForm, htmlBody: e.target.value })} placeholder="HTML template with {{variables}}..." style={{ fontFamily: 'monospace', fontSize: '0.85rem' }} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Plain Text Body</Form.Label>
              <Form.Control as="textarea" rows={3} value={templateForm.textBody} onChange={(e) => setTemplateForm({ ...templateForm, textBody: e.target.value })} placeholder="Fallback plain text version..." />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreateModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreate}>Create Template</Button>
        </Modal.Footer>
      </Modal>

      {/* Preview Modal */}
      <Modal show={showPreviewModal} onHide={() => setShowPreviewModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Email Preview</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div dangerouslySetInnerHTML={{ __html: previewHtml }} style={{ border: '1px solid #e2e8f0', padding: '20px', borderRadius: '8px' }} />
        </Modal.Body>
      </Modal>
    </>
  );
};

export default EmailTemplatesPage;
