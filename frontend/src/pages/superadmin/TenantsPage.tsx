import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Modal, Form, Alert } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { StatusBadge } from '../../components/common/StatusBadge';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { superAdminApi, CreateTenantAdminRequest } from '../../api/superAdminApi';
import { Tenant } from '../../types/tenant';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { useAuth } from '../../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

export const TenantsPage: React.FC = () => {
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<{ type: string; text: string } | null>(null);

  // Create Admin Modal
  const [showAdminModal, setShowAdminModal] = useState(false);
  const [adminTenantId, setAdminTenantId] = useState('');
  const [adminTenantName, setAdminTenantName] = useState('');
  const [adminForm, setAdminForm] = useState<CreateTenantAdminRequest>({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  });

  // Create Tenant Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm, setCreateForm] = useState({ name: '', slug: '', description: '', plan: 'free' });

  const { startImpersonation } = useAuth();
  const navigate = useNavigate();

  const fetchTenants = async () => {
    try {
      setIsLoading(true);
      const filters: any = {};
      if (search) filters.search = search;
      const res = await superAdminApi.getTenants(page, pageSize, filters);
      if (res.data.isValid) {
        setTenants(res.data.data || []);
      }
    } catch (err) {
      console.error('Failed to fetch tenants:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTenants();
  }, [page, search]);

  const handleImpersonate = async (tenantId: string, tenantName: string) => {
    try {
      const res = await superAdminApi.impersonateTenant(tenantId);
      if (res.data.isValid && res.data.data) {
        startImpersonation(res.data.data.accessToken, tenantId, tenantName);
        navigate('/');
      } else {
        setMessage({ type: 'danger', text: res.data.message || 'Failed to impersonate' });
      }
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to impersonate tenant' });
    }
  };

  const handleCreateAdmin = async () => {
    try {
      const res = await superAdminApi.createTenantAdmin(adminTenantId, adminForm);
      if (res.data.isValid) {
        setMessage({ type: 'success', text: `Admin user created for ${adminTenantName}` });
        setShowAdminModal(false);
        setAdminForm({ email: '', password: '', firstName: '', lastName: '' });
      } else {
        setMessage({ type: 'danger', text: res.data.message || 'Failed to create admin' });
      }
    } catch (err: any) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to create admin user' });
    }
  };

  const handleCreateTenant = async () => {
    try {
      const res = await superAdminApi.createTenant(createForm);
      if (res.data.isValid) {
        setMessage({ type: 'success', text: 'Tenant created successfully' });
        setShowCreateModal(false);
        setCreateForm({ name: '', slug: '', description: '', plan: 'free' });
        fetchTenants();
      } else {
        setMessage({ type: 'danger', text: res.data.message || 'Failed to create tenant' });
      }
    } catch (err: any) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to create tenant' });
    }
  };

  const columns: DataTableColumn<Tenant>[] = [
    { key: 'name', label: 'Name' },
    {
      key: 'totalRevenue',
      label: 'Revenue',
      render: (value) => formatCurrency(value),
    },
    {
      key: 'activeSubscriptions',
      label: 'Active Subs',
      render: (value) => <span className="badge bg-info">{value}</span>,
    },
    {
      key: 'totalCustomers',
      label: 'Customers',
      render: (value) => <span className="badge bg-secondary">{value}</span>,
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'Active' : 'Inactive'} />,
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_: any, row: Tenant) => (
        <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap' }}>
          <Button
            size="sm"
            variant="outline-primary"
            onClick={() => handleImpersonate(row.id, row.name)}
          >
            View as Tenant
          </Button>
          <Button
            size="sm"
            variant="outline-success"
            onClick={() => {
              setAdminTenantId(row.id);
              setAdminTenantName(row.name);
              setShowAdminModal(true);
            }}
          >
            Create Admin
          </Button>
        </div>
      ),
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h2>Tenants</h2>
          <Button variant="primary" onClick={() => setShowCreateModal(true)}>Create Tenant</Button>
        </div>

        {message && <Alert variant={message.type} onClose={() => setMessage(null)} dismissible>{message.text}</Alert>}

        <Row className="mb-4">
          <Col md={12}>
            <SearchInput placeholder="Search by tenant name..." onSearch={setSearch} />
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
            <DataTable<Tenant>
              columns={columns}
              data={tenants}
              rowKey="id"
            />
          )}
        </div>
      </div>

      {/* Create Admin Modal */}
      <Modal show={showAdminModal} onHide={() => setShowAdminModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Create Admin for {adminTenantName}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Email</Form.Label>
              <Form.Control
                type="email"
                value={adminForm.email}
                onChange={(e) => setAdminForm({ ...adminForm, email: e.target.value })}
                placeholder="admin@example.com"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Password</Form.Label>
              <Form.Control
                type="password"
                value={adminForm.password}
                onChange={(e) => setAdminForm({ ...adminForm, password: e.target.value })}
                placeholder="Min 6 characters"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>First Name</Form.Label>
              <Form.Control
                value={adminForm.firstName || ''}
                onChange={(e) => setAdminForm({ ...adminForm, firstName: e.target.value })}
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Last Name</Form.Label>
              <Form.Control
                value={adminForm.lastName || ''}
                onChange={(e) => setAdminForm({ ...adminForm, lastName: e.target.value })}
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowAdminModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreateAdmin}>Create Admin</Button>
        </Modal.Footer>
      </Modal>

      {/* Create Tenant Modal */}
      <Modal show={showCreateModal} onHide={() => setShowCreateModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Create New Tenant</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Tenant Name</Form.Label>
              <Form.Control
                value={createForm.name}
                onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })}
                placeholder="My Company"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Slug</Form.Label>
              <Form.Control
                value={createForm.slug}
                onChange={(e) => setCreateForm({ ...createForm, slug: e.target.value })}
                placeholder="my-company"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control
                as="textarea"
                rows={2}
                value={createForm.description}
                onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })}
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Plan</Form.Label>
              <Form.Select
                value={createForm.plan}
                onChange={(e) => setCreateForm({ ...createForm, plan: e.target.value })}
              >
                <option value="free">Free</option>
                <option value="starter">Starter</option>
                <option value="professional">Professional</option>
                <option value="enterprise">Enterprise</option>
              </Form.Select>
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreateModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreateTenant}>Create Tenant</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default TenantsPage;
