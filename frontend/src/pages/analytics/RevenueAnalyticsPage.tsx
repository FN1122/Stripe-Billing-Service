import React, { useEffect, useState } from 'react';
import { Row, Col } from 'react-bootstrap';
import { MetricCard } from '../../components/common/MetricCard';
import { RevenueChart } from '../../components/common/RevenueChart';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { analyticsApi } from '../../api/analyticsApi';
import { MrrData, ChurnData, LtvData } from '../../types/analytics';
import { formatCurrency, formatPercentage } from '../../utils/formatters';

export const RevenueAnalyticsPage: React.FC = () => {
  const [mrrData, setMrrData] = useState<MrrData | null>(null);
  const [churnData, setChurnData] = useState<ChurnData | null>(null);
  const [ltvData, setLtvData] = useState<LtvData | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setIsLoading(true);
        const [mrrRes, churnRes, ltvRes] = await Promise.all([
          analyticsApi.getMrrData(),
          analyticsApi.getChurnData(),
          analyticsApi.getLtvData(),
        ]);

        if (mrrRes.data.isValid) setMrrData(mrrRes.data.data);
        if (churnRes.data.isValid) setChurnData(churnRes.data.data);
        if (ltvRes.data.isValid) setLtvData(ltvRes.data.data);
      } catch (err) {
        console.error('Failed to fetch analytics:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '30px' }}>Revenue Analytics</h2>

        {isLoading ? (
          <LoadingSkeleton count={8} height={100} />
        ) : (
          <>
            <Row className="mb-4">
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Current MRR"
                  value={formatCurrency(mrrData?.currentMrr || 0)}
                  change={mrrData?.mrrGrowth}
                  trend={(mrrData?.mrrGrowth || 0) >= 0 ? 'up' : 'down'}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Monthly Churn Rate"
                  value={formatPercentage(churnData?.monthlyChurnRate || 0)}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Average LTV"
                  value={formatCurrency(ltvData?.averageLtv || 0)}
                />
              </Col>
              <Col lg={3} md={6} className="mb-3">
                <MetricCard
                  label="Retention Rate"
                  value={formatPercentage(churnData?.retentionRate || 0)}
                />
              </Col>
            </Row>

            <Row className="mb-4">
              <Col lg={12}>
                <div style={{
                  background: 'white',
                  padding: '20px',
                  borderRadius: '8px',
                  border: '1px solid #e2e8f0',
                }}>
                  <h4 style={{ marginBottom: '20px' }}>MRR Trend</h4>
                  {mrrData?.mrrHistory && (
                    <RevenueChart data={mrrData.mrrHistory} title="Monthly Recurring Revenue" />
                  )}
                </div>
              </Col>
            </Row>

            <Row>
              <Col lg={6} className="mb-4">
                <div style={{
                  background: 'white',
                  padding: '20px',
                  borderRadius: '8px',
                  border: '1px solid #e2e8f0',
                }}>
                  <h5>MRR Components</h5>
                  <p><strong>New MRR:</strong> {formatCurrency(mrrData?.newMrr || 0)}</p>
                  <p><strong>Expansion MRR:</strong> {formatCurrency(mrrData?.expansionMrr || 0)}</p>
                  <p><strong>Contraction MRR:</strong> {formatCurrency(mrrData?.contractionMrr || 0)}</p>
                  <p><strong>Churned MRR:</strong> {formatCurrency(mrrData?.churnedMrr || 0)}</p>
                </div>
              </Col>
              <Col lg={6} className="mb-4">
                <div style={{
                  background: 'white',
                  padding: '20px',
                  borderRadius: '8px',
                  border: '1px solid #e2e8f0',
                }}>
                  <h5>Customer Metrics</h5>
                  <p><strong>Avg Subscription Duration:</strong> {ltvData?.averageSubscriptionDurationMonths || 0} months</p>
                  <p><strong>Avg Revenue Per Customer:</strong> {formatCurrency(ltvData?.averageRevenuePerCustomer || 0)}</p>
                  <p><strong>Churn Rate (Annual):</strong> {formatPercentage(churnData?.annualChurnRate || 0)}</p>
                  <p><strong>Churned This Month:</strong> {churnData?.churnedSubscriptions || 0} subscriptions</p>
                </div>
              </Col>
            </Row>
          </>
        )}
      </div>
    </>
  );
};

export default RevenueAnalyticsPage;
