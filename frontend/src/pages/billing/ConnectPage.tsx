import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Modal, Form, Badge } from 'react-bootstrap';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { StatusBadge } from '../../components/common/StatusBadge';
import { connectApi } from '../../api/connectApi';
import { ConnectedAccountResponse, TransferResponse } from '../../types/connect';
import { Link2, Plus, Send, DollarSign, Building, Users } from 'lucide-react';

export const ConnectPage: React.FC = () => {
  const [accounts, setAccounts] = useState<ConnectedAccountResponse[]>([]);
  const [transfers, setTransfers] = useState<TransferResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [createForm, setCreateForm] = useState({
    email: '',
    businessType: 'individual',
    country: 'US',
  });
  const [transferForm, setTransferForm] = useState({
    connectedAccountId: '',
    amount: 0,
    currency: 'usd',
    description: '',
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [accountsRes, transfersRes] = await Promise.all([
        connectApi.getAccounts(),
        connectApi.getTransfers(),
      ]);
      if (accountsRes.data.isValid) setAccounts(accountsRes.data.data || []);
      if (transfersRes.data.isValid) setTransfers(transfersRes.data.data || []);
    } catch (err) {
      console.error('Failed to fetch connect data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateAccount = async () => {
    try {
      await connectApi.createAccount(createForm);
      setShowCreateModal(false);
      fetchData();
    } catch (err) {
      console.error('Failed to create account:', err);
    }
  };

  const handleCreateTransfer = async () => {
    try {
      await connectApi.createTransfer(transferForm);
      setShowTransferModal(false);
      fetchData();
    } catch (err) {
      console.error('Failed to create transfer:', err);
    }
  };

  const formatAmount = (amount: number, currency: string = 'USD') => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount / 100);
  };

  const accountColumns: DataTableColumn<ConnectedAccountResponse>[] = [
    {
      key: 'email',
      label: 'Account',
      render: (_, row) => (
        <div>
          <div className="fw-medium">{row.email}</div>
          <small className="text-muted">{row.stripeAccountId}</small>
        </div>
      ),
    },
    {
      key: 'type',
      label: 'Type',
      render: (value) => <Badge bg="info" className="text-capitalize">{value}</Badge>,
    },
    {
      key: 'country',
      label: 'Country',
    },
    {
      key: 'chargesEnabled',
      label: 'Charges',
      render: (value) => <StatusBadge status={value ? 'active' : 'inactive'} />,
    },
    {
      key: 'payoutsEnabled',
      label: 'Payouts',
      render: (value) => <StatusBadge status={value ? 'active' : 'inactive'} />,
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_, row) => (
        <Button size="sm" variant="outline-primary" onClick={() => {
          setTransferForm({ ...transferForm, connectedAccountId: row.id });
          setShowTransferModal(true);
        }}>
          <Send size={14} className="me-1" /> Transfer
        </Button>
      ),
    },
  ];

  const transferColumns: DataTableColumn<TransferResponse>[] = [
    {
      key: 'connectedAccountId',
      label: 'Account',
      render: (value) => <code className="bg-light px-2 py-1 rounded" style={{ fontSize: '0.8rem' }}>{value?.substring(0, 12)}...</code>,
    },
    {
      key: 'amount',
      label: 'Amount',
      render: (value, row) => <span className="fw-medium">{formatAmount(value, row.currency)}</span>,
    },
    {
      key: 'status',
      label: 'Status',
      render: (value) => <StatusBadge status={value} />,
    },
    {
      key: 'description',
      label: 'Description',
      render: (value) => <span className="text-muted">{value || '-'}</span>,
    },
    {
      key: 'createdAt',
      label: 'Date',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Stripe Connect</h2>

        {/* Stats */}
        <Row className="mb-4">
          <Col md={4}>
            <Card className="border-0 shadow-sm h-100">
              <Card.Body className="text-center">
                <Building size={24} className="text-primary mb-2" />
                <h6 className="text-muted">Connected Accounts</h6>
                <h3>{accounts.length}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card className="border-0 shadow-sm h-100">
              <Card.Body className="text-center">
                <Send size={24} className="text-success mb-2" />
                <h6 className="text-muted">Total Transfers</h6>
                <h3>{transfers.length}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card className="border-0 shadow-sm h-100">
              <Card.Body className="text-center">
                <DollarSign size={24} className="text-info mb-2" />
                <h6 className="text-muted">Transfer Volume</h6>
                <h3>{formatAmount(transfers.reduce((sum, t) => sum + (t.amount || 0), 0))}</h3>
              </Card.Body>
            </Card>
          </Col>
        </Row>

        {/* Accounts */}
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5>Connected Accounts</h5>
          <Button variant="primary" onClick={() => setShowCreateModal(true)}>
            <Plus size={16} className="me-1" /> Add Account
          </Button>
        </div>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0', marginBottom: '24px' }}>
          {isLoading ? <LoadingSkeleton count={3} height={50} /> : (
            <DataTable<ConnectedAccountResponse> columns={accountColumns} data={accounts} rowKey="id" />
          )}
        </div>

        {/* Transfers */}
        <h5 className="mb-3">Recent Transfers</h5>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
          {isLoading ? <LoadingSkeleton count={3} height={50} /> : (
            <DataTable<TransferResponse> columns={transferColumns} data={transfers} rowKey="id" />
          )}
        </div>
      </div>

      {/* Create Account Modal */}
      <Modal show={showCreateModal} onHide={() => setShowCreateModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Add Connected Account</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Email</Form.Label>
              <Form.Control type="email" value={createForm.email} onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })} placeholder="account@example.com" />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Business Type</Form.Label>
              <Form.Select value={createForm.businessType} onChange={(e) => setCreateForm({ ...createForm, businessType: e.target.value })}>
                <option value="individual">Individual</option>
                <option value="company">Company</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Country</Form.Label>
              <Form.Control value={createForm.country} onChange={(e) => setCreateForm({ ...createForm, country: e.target.value })} placeholder="US" />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreateModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreateAccount}>Create Account</Button>
        </Modal.Footer>
      </Modal>

      {/* Create Transfer Modal */}
      <Modal show={showTransferModal} onHide={() => setShowTransferModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Create Transfer</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Amount (cents)</Form.Label>
              <Form.Control type="number" min="1" value={transferForm.amount} onChange={(e) => setTransferForm({ ...transferForm, amount: Number(e.target.value) })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Currency</Form.Label>
              <Form.Select value={transferForm.currency} onChange={(e) => setTransferForm({ ...transferForm, currency: e.target.value })}>
                <option value="usd">USD</option>
                <option value="eur">EUR</option>
                <option value="gbp">GBP</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control value={transferForm.description} onChange={(e) => setTransferForm({ ...transferForm, description: e.target.value })} placeholder="Payment for services" />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowTransferModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreateTransfer}>Send Transfer</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ConnectPage;
