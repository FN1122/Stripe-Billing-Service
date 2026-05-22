import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { PlanCard } from '../../components/common/PlanCard';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { subscriptionApi } from '../../api/subscriptionApi';
import { SubscriptionPlan } from '../../types/subscription';

export const PlansPage: React.FC = () => {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchPlans = async () => {
      try {
        setIsLoading(true);
        const res = await subscriptionApi.getPlans();
        if (res.data.isValid) {
          setPlans(res.data.data || []);
        }
      } catch (err) {
        console.error('Failed to fetch plans:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPlans();
  }, []);

  return (
    <>
      <div style={{ padding: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '30px' }}>
          <h2>Subscription Plans</h2>
          <Button variant="primary">Create Plan</Button>
        </div>

        {isLoading ? (
          <Row>
            {[1, 2, 3].map((i) => (
              <Col lg={4} md={6} key={i} className="mb-4">
                <LoadingSkeleton height={400} />
              </Col>
            ))}
          </Row>
        ) : (
          <Row>
            {plans.map((plan) => (
              <Col lg={4} md={6} key={plan.id} className="mb-4">
                <PlanCard
                  plan={plan}
                  onEdit={() => console.log('Edit plan:', plan.id)}
                />
              </Col>
            ))}
          </Row>
        )}
      </div>
    </>
  );
};

export default PlansPage;
