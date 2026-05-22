import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Modal, Form, Badge, Tab, Tabs } from 'react-bootstrap';
import { SearchInput } from '../../components/common/SearchInput';
import { DataTable, DataTableColumn } from '../../components/common/DataTable';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { StatusBadge } from '../../components/common/StatusBadge';
import { couponApi } from '../../api/couponApi';
import { CouponResponse, PromotionCodeResponse } from '../../types/coupon';
import { Tag, Percent, Plus, Copy } from 'lucide-react';

export const CouponsPage: React.FC = () => {
  const [coupons, setCoupons] = useState<CouponResponse[]>([]);
  const [promotionCodes, setPromotionCodes] = useState<PromotionCodeResponse[]>([]);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('coupons');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showPromoModal, setShowPromoModal] = useState(false);

  // Create coupon form state
  const [couponForm, setCouponForm] = useState({
    name: '', type: 'percent_off', percentOff: 0, amountOff: 0,
    currency: 'usd', duration: 'once', durationInMonths: 0, maxRedemptions: 0,
  });

  // Create promo code form state
  const [promoForm, setPromoForm] = useState({
    couponId: '', code: '', maxRedemptions: 0, firstTimeTransaction: false,
  });

  useEffect(() => {
    fetchData();
  }, [search]);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const couponsRes = await couponApi.getCoupons({ page: 1, pageSize: 50, search });
      if (couponsRes.data.isValid) {
        const couponsData = couponsRes.data.data || [];
        setCoupons(couponsData);

        // Fetch promotion codes for each coupon
        const allPromos: PromotionCodeResponse[] = [];
        const promoPromises = couponsData.slice(0, 10).map(async (coupon) => {
          try {
            const promosRes = await couponApi.getPromotionCodes(coupon.id);
            if (promosRes.data.isValid && promosRes.data.data) {
              allPromos.push(...promosRes.data.data);
            }
          } catch {
            // Skip if promotion codes fetch fails for a coupon
          }
        });
        await Promise.all(promoPromises);
        setPromotionCodes(allPromos);
      }
    } catch (err) {
      console.error('Failed to fetch coupons:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateCoupon = async () => {
    try {
      const dto = {
        name: couponForm.name,
        type: couponForm.type,
        percentOff: couponForm.type === 'percent_off' ? couponForm.percentOff : undefined,
        amountOff: couponForm.type === 'amount_off' ? couponForm.amountOff : undefined,
        currency: couponForm.type === 'amount_off' ? couponForm.currency : undefined,
        duration: couponForm.duration,
        durationInMonths: couponForm.duration === 'repeating' ? couponForm.durationInMonths : undefined,
        maxRedemptions: couponForm.maxRedemptions || undefined,
      };
      await couponApi.createCoupon(dto);
      setShowCreateModal(false);
      fetchData();
    } catch (err) {
      console.error('Failed to create coupon:', err);
    }
  };

  const handleCreatePromo = async () => {
    try {
      const { couponId, ...promoData } = promoForm;
      await couponApi.createPromotionCode(couponId, promoData);
      setShowPromoModal(false);
      fetchData();
    } catch (err) {
      console.error('Failed to create promotion code:', err);
    }
  };

  const handleDeactivate = async (id: string) => {
    try {
      await couponApi.toggleCoupon(id);
      fetchData();
    } catch (err) {
      console.error('Failed to deactivate coupon:', err);
    }
  };

  const couponColumns: DataTableColumn<CouponResponse>[] = [
    {
      key: 'name',
      label: 'Coupon',
      render: (_, row) => (
        <div>
          <div className="fw-medium">{row.name}</div>
          <small className="text-muted">{row.stripeCouponId}</small>
        </div>
      ),
    },
    {
      key: 'type',
      label: 'Discount',
      render: (_, row) => (
        <Badge bg="primary" className="d-inline-flex align-items-center gap-1">
          {row.type === 'percent_off' ? <Percent size={12} /> : <Tag size={12} />}
          {row.type === 'percent_off' ? `${row.percentOff}% off` : `$${((row.amountOff || 0) / 100).toFixed(2)} off`}
        </Badge>
      ),
    },
    {
      key: 'duration',
      label: 'Duration',
      render: (value) => <span className="text-capitalize">{value}</span>,
    },
    {
      key: 'timesRedeemed',
      label: 'Redeemed',
      render: (value, row) => (
        <span>{value}{row.maxRedemptions ? ` / ${row.maxRedemptions}` : ''}</span>
      ),
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'active' : 'inactive'} />,
    },
    {
      key: 'id',
      label: 'Actions',
      render: (_, row) => (
        <div className="d-flex gap-1">
          <Button size="sm" variant="outline-secondary" onClick={() => { setPromoForm({ ...promoForm, couponId: row.id }); setShowPromoModal(true); }}>
            <Copy size={14} /> Promo
          </Button>
          {row.isActive && (
            <Button size="sm" variant="outline-danger" onClick={() => handleDeactivate(row.id)}>
              Deactivate
            </Button>
          )}
        </div>
      ),
    },
  ];

  const promoColumns: DataTableColumn<PromotionCodeResponse>[] = [
    { key: 'code', label: 'Code', render: (value) => <code className="bg-light px-2 py-1 rounded">{value}</code> },
    { key: 'couponName', label: 'Coupon' },
    {
      key: 'timesRedeemed',
      label: 'Redeemed',
      render: (value, row) => <span>{value}{row.maxRedemptions ? ` / ${row.maxRedemptions}` : ''}</span>,
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (value) => <StatusBadge status={value ? 'active' : 'inactive'} />,
    },
    {
      key: 'expiresAt',
      label: 'Expires',
      render: (value) => value ? new Date(value).toLocaleDateString() : 'Never',
    },
  ];

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Coupons & Promotions</h2>

        <Tabs activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'coupons')} className="mb-4">
          <Tab eventKey="coupons" title={`Coupons (${coupons.length})`}>
            <Row className="mb-4">
              <Col md={8}>
                <SearchInput placeholder="Search coupons..." onSearch={setSearch} />
              </Col>
              <Col md={4} className="text-end">
                <Button variant="primary" onClick={() => setShowCreateModal(true)}>
                  <Plus size={16} className="me-1" /> Create Coupon
                </Button>
              </Col>
            </Row>

            <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              {isLoading ? <LoadingSkeleton count={5} height={50} /> : (
                <DataTable<CouponResponse> columns={couponColumns} data={coupons} rowKey="id" />
              )}
            </div>
          </Tab>

          <Tab eventKey="promotions" title={`Promotion Codes (${promotionCodes.length})`}>
            <div style={{ background: 'white', padding: '20px', borderRadius: '8px', border: '1px solid #e2e8f0', marginTop: '16px' }}>
              {isLoading ? <LoadingSkeleton count={5} height={50} /> : (
                <DataTable<PromotionCodeResponse> columns={promoColumns} data={promotionCodes} rowKey="id" />
              )}
            </div>
          </Tab>
        </Tabs>
      </div>

      {/* Create Coupon Modal */}
      <Modal show={showCreateModal} onHide={() => setShowCreateModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Create Coupon</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control value={couponForm.name} onChange={(e) => setCouponForm({ ...couponForm, name: e.target.value })} placeholder="e.g. Summer Sale 20%" />
            </Form.Group>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Type</Form.Label>
                  <Form.Select value={couponForm.type} onChange={(e) => setCouponForm({ ...couponForm, type: e.target.value })}>
                    <option value="percent_off">Percentage Off</option>
                    <option value="amount_off">Fixed Amount Off</option>
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                {couponForm.type === 'percent_off' ? (
                  <Form.Group className="mb-3">
                    <Form.Label>Percent Off</Form.Label>
                    <Form.Control type="number" min="1" max="100" value={couponForm.percentOff} onChange={(e) => setCouponForm({ ...couponForm, percentOff: Number(e.target.value) })} />
                  </Form.Group>
                ) : (
                  <Form.Group className="mb-3">
                    <Form.Label>Amount Off (cents)</Form.Label>
                    <Form.Control type="number" min="1" value={couponForm.amountOff} onChange={(e) => setCouponForm({ ...couponForm, amountOff: Number(e.target.value) })} />
                  </Form.Group>
                )}
              </Col>
            </Row>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Duration</Form.Label>
                  <Form.Select value={couponForm.duration} onChange={(e) => setCouponForm({ ...couponForm, duration: e.target.value })}>
                    <option value="once">Once</option>
                    <option value="repeating">Repeating</option>
                    <option value="forever">Forever</option>
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Max Redemptions (0 = unlimited)</Form.Label>
                  <Form.Control type="number" min="0" value={couponForm.maxRedemptions} onChange={(e) => setCouponForm({ ...couponForm, maxRedemptions: Number(e.target.value) })} />
                </Form.Group>
              </Col>
            </Row>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreateModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreateCoupon}>Create Coupon</Button>
        </Modal.Footer>
      </Modal>

      {/* Create Promotion Code Modal */}
      <Modal show={showPromoModal} onHide={() => setShowPromoModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Create Promotion Code</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Code</Form.Label>
              <Form.Control value={promoForm.code} onChange={(e) => setPromoForm({ ...promoForm, code: e.target.value })} placeholder="e.g. SUMMER2025" />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Max Redemptions (0 = unlimited)</Form.Label>
              <Form.Control type="number" min="0" value={promoForm.maxRedemptions} onChange={(e) => setPromoForm({ ...promoForm, maxRedemptions: Number(e.target.value) })} />
            </Form.Group>
            <Form.Check type="checkbox" label="First-time customers only" checked={promoForm.firstTimeTransaction} onChange={(e) => setPromoForm({ ...promoForm, firstTimeTransaction: e.target.checked })} />
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowPromoModal(false)}>Cancel</Button>
          <Button variant="primary" onClick={handleCreatePromo}>Create Code</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default CouponsPage;
