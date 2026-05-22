import React from 'react';
import { Button } from 'react-bootstrap';
import { Check } from 'lucide-react';
import { SubscriptionPlan } from '../../types/subscription';
import { formatCurrency } from '../../utils/formatters';
import './PlanCard.scss';

interface PlanCardProps {
  plan: SubscriptionPlan;
  isSelected?: boolean;
  onSelect?: () => void;
  onEdit?: () => void;
}

export const PlanCard: React.FC<PlanCardProps> = ({
  plan,
  isSelected = false,
  onSelect,
  onEdit,
}) => {
  return (
    <div className={`plan-card ${isSelected ? 'selected' : ''}`}>
      <div className="plan-header">
        <h3 className="plan-name">{plan.name}</h3>
        {!plan.isActive && <span className="plan-inactive">Inactive</span>}
      </div>

      {plan.description && <p className="plan-description">{plan.description}</p>}

      <div className="plan-price">
        <span className="price-amount">{formatCurrency(plan.amount, plan.currency)}</span>
        <span className="price-interval">/ {plan.interval}</span>
      </div>

      {plan.trialDays > 0 && <p className="plan-trial">{plan.trialDays} days free trial</p>}

      {plan.features.length > 0 && (
        <ul className="plan-features">
          {plan.features.slice(0, 5).map((feature, idx) => (
            <li key={idx}>
              <Check size={16} />
              <span>{feature}</span>
            </li>
          ))}
          {plan.features.length > 5 && <li>+ {plan.features.length - 5} more</li>}
        </ul>
      )}

      <div className="plan-stats">
        <span className="stat">{plan.subscriberCount} subscribers</span>
      </div>

      <div className="plan-actions">
        {onSelect && <Button variant="outline-primary" onClick={onSelect}>
          Select
        </Button>}
        {onEdit && <Button variant="secondary" onClick={onEdit}>
          Edit
        </Button>}
      </div>
    </div>
  );
};
