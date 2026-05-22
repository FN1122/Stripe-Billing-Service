import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Modal, Form, Badge } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { creditApi } from '../../api/creditApi';
import { CreditTransaction, CreditsDashboard } from '../../types/credit';
import { Wallet, Plus, TrendingUp, TrendingDown, DollarSign } from 'lucide-react';

export const CreditsPage: React.FC = () => {
  const [dashboard, setDashboard] = useState<CreditsDashboard | null>(null);
  const [credits, setCredits] = useState<CreditTransaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [addForm, setAddForm] = useState({
    customerId: '',
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
      const dashRes = await creditApi.getDashboard();
      if (dashRes.data.isValid) setDashboard(dashRes.data.data);
      const txRes = await creditApi.getTransactions();
      if (txRes.data.isValid) setCredits(txRes.data.data || []);
    } catch (err) {
      console.error('Failed to fetch credits:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddCredit = async () => {
    try {
      const { customerId, ...creditData } = addForm;
      await creditApi.addCredit(customerId, creditData);
      setShowAddModal(false);
      setAddForm({ customerId: '', amount: 0, currency: 'usd', description: '' });
      fetchData();
    } catch (err) {
      console.error('Failed to add credit:', err);
    }
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount / 100);
  };

  const columns: DataTableColumn<CreditTransaction>[] = [
    {
      key: 'customerId',
      label: 'Customer',
      render: (value) => <code className="bg-light px-2 py-1 rounded" style={{ fontSize: '0.8rem' }}>{value?.substring(0, 12)}...</code>,
    },
    {
      key: 'amount',
      label: 'Amount',
      render: (value, row) => (
        <span className={`fw-medium ${row.type === 'credit' ? 'text-success' : 'text-danger'}`}>
          {row.type === 'credit' ? '+' : '-'}{formatAmount(Math.abs(value))}
        </span>
      ),
    },
    {
      key: 'type',
      label: 'Type',
      render: (value) => (
        <Badge bg={value === 'credit' ? 'success' : 'danger'}>
          {value === 'credit' ? <TrendingUp size={12} className="me-1" /> : <TrendingDown size={12} className="me-1" />}
          {value}
        </Badge>
      ),
    },
    {
      key: 'description',
      label: 'Description',
      render: (value) => <span className="text-muted">{value || '-'}</span>,
    },
    {
      key: 'balanceAfter',
      label: 'Balance After',
      render: (value) => formatAmount(value),
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
        <div className="d-flex justify-content-between align-items-center mb-4">
          <h2>Customer Credits</h2>
          <Button variant="primary" onClick={() => setShowAddModal(true)}>
            <Plus size={16} className="me-1" /> Add Credit
          </Button>
        </div>

        {/* Dashboard */}
        {isLoading ? (
          <LoadingSkeleton count={3} height={100} />
        ) : dashboard && (
          <Row className="mb-4">
            <Col md={4}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <Wallet size={24} className="text-primary mb-2" />
                  <h6 className="text-muted">Total Outstanding</h6>
                  <h3>{formatAmount(dashboard.totalOutstandingCredits || 0)}</h3>
                </Card.Body>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <TrendingUp size={24} className="text-success mb-2" />
                  <h6 className="text-muted">Total Credits Issued</h6>
                  <h3>{formatAmount(dashboard.totalCreditsIssued || 0)}</h3>
                </Card.Body>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="text-center">
                  <DollarSign size={24} className="text-info mb-2" />
                  <h6 className="text-muted">Customers with Balance</h6>
                  <h3>{dashboard.customersWithCredits || 0}</h3>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        )}

        {/* Credits Table */}
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
          {isLoading ? (
            <LoadingSkeleton count={5} height={50} />
          ) : (
            <DataTable<CreditTransaction>
              columns={columns}
              data={credits}
              rowKey="id"
            />
          )}
        </div>
      </div>

      {/* Add Credit Modal */}
      <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Add Customer Credit</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Customer ID</Form.Label>
              <Form.Control value={addForm.customerId} onChange={(e) => setAddForm({ ...addForm, customerId: e.target.value })} placeholder="Enter customer ID" />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Amount (in cents)</Form.Label>
              <Form.Control type="number" min="1" value={addForm.amount} onChange={(e) => setAddForm({ ...addForm, amount: Number(e.target.value) })} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Currency</Form.Label>
              <Form.Select value={addForm.currency} onChange={(e) => setAddForm({ ...addForm, currency: e.target.value })}>
                <option value="usd">USD</option>
                <option value="eur">EUR</option>
                <option value="gbp">GBP</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control as="textarea" rows={2} value={addForm.description} onChange={(e) => setAddForm({ ...addForm, description: e.target.value })} placeholder="Reason for credit..." />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowAddModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleAddCredit}>Add Credit</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default CreditsPage;
