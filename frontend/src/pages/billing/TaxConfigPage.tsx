import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Form, Button, Badge, Tab, Tabs, Modal } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { StatusBadge } from '../../components/common/StatusBadge';
import { taxApi } from '../../api/taxApi';
import { TaxConfigurationResponse } from '../../types/tax';
import { Receipt, Settings, Shield, Plus } from 'lucide-react';

export const TaxConfigPage: React.FC = () => {
  const [config, setConfig] = useState<TaxConfigurationResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editForm, setEditForm] = useState({
    provider: 'stripe_tax',
    isEnabled: false,
    autoCalculate: true,
    defaultTaxBehavior: 'exclusive',
    fallbackTaxRate: 0,
  });

  useEffect(() => {
    fetchConfig();
  }, []);

  const fetchConfig = async () => {
    try {
      setIsLoading(true);
      const res = await taxApi.getConfiguration();
      if (res.data.isValid && res.data.data) {
        setConfig(res.data.data);
        setEditForm({
          provider: res.data.data.provider,
          isEnabled: res.data.data.isEnabled,
          autoCalculate: res.data.data.autoCalculate,
          defaultTaxBehavior: res.data.data.defaultTaxBehavior,
          fallbackTaxRate: res.data.data.fallbackTaxRate || 0,
        });
      }
    } catch (err) {
      console.error('Failed to fetch tax config:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdateConfig = async () => {
    try {
      await taxApi.updateConfiguration(editForm);
      setShowEditModal(false);
      fetchConfig();
    } catch (err) {
      console.error('Failed to update tax config:', err);
    }
  };

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div className="d-flex justify-content-between align-items-center mb-4">
          <h2>Tax Configuration</h2>
          <Button variant="primary" onClick={() => setShowEditModal(true)}>
            <Settings size={16} className="me-1" /> Edit Configuration
          </Button>
        </div>

        {isLoading ? (
          <LoadingSkeleton count={3} height={100} />
        ) : config ? (
          <>
            {/* Config Overview */}
            <Row className="mb-4">
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Receipt size={24} className="text-primary mb-2" />
                    <h6 className="text-muted">Provider</h6>
                    <Badge bg="info" className="text-capitalize">{config.provider?.replace('_', ' ')}</Badge>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Shield size={24} className={config.isEnabled ? 'text-success' : 'text-muted'} />
                    <h6 className="text-muted mt-2">Status</h6>
                    <StatusBadge status={config.isEnabled ? 'active' : 'inactive'} />
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Settings size={24} className="text-warning mb-2" />
                    <h6 className="text-muted">Tax Behavior</h6>
                    <Badge bg="secondary" className="text-capitalize">{config.defaultTaxBehavior}</Badge>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={3}>
                <Card className="border-0 shadow-sm h-100">
                  <Card.Body className="text-center">
                    <Receipt size={24} className="text-info mb-2" />
                    <h6 className="text-muted">Fallback Rate</h6>
                    <h4>{config.fallbackTaxRate ? `${(config.fallbackTaxRate * 100).toFixed(2)}%` : 'None'}</h4>
                  </Card.Body>
                </Card>
              </Col>
            </Row>

            {/* Tax Registrations */}
            <Card className="border-0 shadow-sm mb-4">
              <Card.Body>
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h5 className="mb-0">Tax Registrations</h5>
                  <Button variant="outline-primary" size="sm"><Plus size={14} className="me-1" /> Add Registration</Button>
                </div>
                {config.registrationNumbers && config.registrationNumbers.length > 0 ? (
                  <div className="d-flex gap-2 flex-wrap">
                    {config.registrationNumbers.map((reg, idx) => (
                      <Badge key={idx} bg="light" text="dark" className="border px-3 py-2">
                        <div className="fw-medium">{reg.country}</div>
                        <small className="text-muted">{reg.type}: {reg.value}</small>
                      </Badge>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted mb-0">No tax registrations configured.</p>
                )}
              </Card.Body>
            </Card>

            {/* Auto Calculate info */}
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <h5 className="mb-3">Settings</h5>
                <div className="d-flex align-items-center gap-2 mb-2">
                  <Badge bg={config.autoCalculate ? 'success' : 'secondary'}>
                    {config.autoCalculate ? 'Enabled' : 'Disabled'}
                  </Badge>
                  <span>Auto-calculate tax on invoices</span>
                </div>
                <small className="text-muted">
                  Last updated: {config.updatedAt ? new Date(config.updatedAt).toLocaleString() : 'Never'}
                </small>
              </Card.Body>
            </Card>
          </>
        ) : (
          <Card className="border-0 shadow-sm">
            <Card.Body className="text-center py-5">
              <Receipt size={48} className="text-muted mb-3" />
              <h5>No Tax Configuration</h5>
              <p className="text-muted">Configure tax settings to start collecting taxes automatically.</p>
              <Button variant="primary" onClick={() => setShowEditModal(true)}>Configure Taxes</Button>
            </Card.Body>
          </Card>
        )}
      </div>

      {/* Edit Config Modal */}
      <Modal show={showEditModal} onHide={() => setShowEditModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Tax Configuration</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Provider</Form.Label>
              <Form.Select value={editForm.provider} onChange={(e) => setEditForm({ ...editForm, provider: e.target.value })}>
                <option value="stripe_tax">Stripe Tax</option>
                <option value="taxjar">TaxJar</option>
                <option value="avalara">Avalara</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Check type="switch" label="Enable Tax Calculation" checked={editForm.isEnabled} onChange={(e) => setEditForm({ ...editForm, isEnabled: e.target.checked })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Check type="switch" label="Auto-Calculate on Invoices" checked={editForm.autoCalculate} onChange={(e) => setEditForm({ ...editForm, autoCalculate: e.target.checked })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Default Tax Behavior</Form.Label>
              <Form.Select value={editForm.defaultTaxBehavior} onChange={(e) => setEditForm({ ...editForm, defaultTaxBehavior: e.target.value })}>
                <option value="exclusive">Exclusive (added on top)</option>
                <option value="inclusive">Inclusive (included in price)</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Fallback Tax Rate (%)</Form.Label>
              <Form.Control type="number" step="0.01" min="0" max="100" value={editForm.fallbackTaxRate * 100} onChange={(e) => setEditForm({ ...editForm, fallbackTaxRate: Number(e.target.value) / 100 })} />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEditModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleUpdateConfig}>Save Changes</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default TaxConfigPage;
