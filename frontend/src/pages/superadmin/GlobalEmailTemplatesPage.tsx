import React, { useEffect, useState } from 'react';
import { Button, Modal, Form, Alert } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { superAdminApi } from '../../api/superAdminApi';

interface EmailTemplate {
  id: string;
  name: string;
  subject: string;
  bodyHtml: string;
  eventType: string;
  isActive: boolean;
}

export const GlobalEmailTemplatesPage: React.FC = () => {
  const [templates, setTemplates] = useState<EmailTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<EmailTemplate | null>(null);
  const [form, setForm] = useState({ name: '', subject: '', bodyHtml: '', eventType: 'invoice.created' });
  const [message, setMessage] = useState<{ type: string; text: string } | null>(null);

  const fetchTemplates = async () => {
    try {
      setIsLoading(true);
      const res = await superAdminApi.getGlobalEmailTemplates();
      if (res.data.isValid) {
        setTemplates(res.data.data || []);
      }
    } catch (err) {
      console.error('Failed to fetch templates:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates();
  }, []);

  const handleSave = async () => {
    try {
      if (editing) {
        await superAdminApi.updateGlobalEmailTemplate(editing.id, form);
      } else {
        await superAdminApi.createGlobalEmailTemplate(form);
      }
      setShowModal(false);
      setEditing(null);
      setMessage({ type: 'success', text: editing ? 'Template updated' : 'Template created' });
      fetchTemplates();
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to save template' });
    }
  };

  const handleEdit = (template: EmailTemplate) => {
    setEditing(template);
    setForm({
      name: template.name,
      subject: template.subject,
      bodyHtml: template.bodyHtml,
      eventType: template.eventType,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this template?')) return;
    try {
      await superAdminApi.deleteGlobalEmailTemplate(id);
      setMessage({ type: 'success', text: 'Template deleted' });
      fetchTemplates();
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to delete template' });
    }
  };

  const columns: DataTableColumn<EmailTemplate>[] = [
    { key: 'name', label: 'Name' },
    { key: 'subject', label: 'Subject' },
    { key: 'eventType', label: 'Event Type' },
    {
      key: 'isActive',
      label: 'Status',
      render: (value: boolean) => (
        <span className={`badge bg-${value ? 'success' : 'secondary'}`}>{value ? 'Active' : 'Inactive'}</span>
      ),
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_: any, row: EmailTemplate) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button size="sm" variant="outline-primary" onClick={() => handleEdit(row)}>Edit</Button>
          <Button size="sm" variant="outline-danger" onClick={() => handleDelete(row.id)}>Delete</Button>
        </div>
      ),
    },
  ];

  return (
    <div style={{ padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Global Email Templates</h2>
        <Button variant="primary" onClick={() => { setEditing(null); setForm({ name: '', subject: '', bodyHtml: '', eventType: 'invoice.created' }); setShowModal(true); }}>
          Create Template
        </Button>
      </div>

      {message && <Alert variant={message.type} onClose={() => setMessage(null)} dismissible>{message.text}</Alert>}

      <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        {isLoading ? (
          <LoadingSkeleton count={4} height={50} />
        ) : (
          <DataTable<EmailTemplate> columns={columns} data={templates} rowKey="id" />
        )}
      </div>

      <Modal show={showModal} onHide={() => setShowModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>{editing ? 'Edit Template' : 'Create Template'}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Event Type</Form.Label>
              <Form.Select value={form.eventType} onChange={(e) => setForm({ ...form, eventType: e.target.value })}>
                <option value="invoice.created">Invoice Created</option>
                <option value="invoice.paid">Invoice Paid</option>
                <option value="invoice.overdue">Invoice Overdue</option>
                <option value="subscription.created">Subscription Created</option>
                <option value="subscription.canceled">Subscription Canceled</option>
                <option value="payment.failed">Payment Failed</option>
                <option value="payment.succeeded">Payment Succeeded</option>
                <option value="customer.created">Customer Created</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Subject</Form.Label>
              <Form.Control value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Body (HTML)</Form.Label>
              <Form.Control as="textarea" rows={6} value={form.bodyHtml} onChange={(e) => setForm({ ...form, bodyHtml: e.target.value })} />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleSave}>Save</Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default GlobalEmailTemplatesPage;
