import React from 'react';
import { Badge } from 'react-bootstrap';
import { Tag, Percent } from 'lucide-react';

interface CouponBadgeProps {
  type: string;
  amountOff?: number;
  percentOff?: number;
  currency?: string;
  duration?: string;
  isActive?: boolean;
}

export const CouponBadge: React.FC<CouponBadgeProps> = ({ type, amountOff, percentOff, currency, duration, isActive = true }) => {
  const getLabel = () => {
    if (type === 'percent_off' && percentOff) return `${percentOff}% off`;
    if (type === 'amount_off' && amountOff) return `${(amountOff / 100).toFixed(2)} ${currency?.toUpperCase() || 'USD'} off`;
    return 'Coupon';
  };

  const getDurationLabel = () => {
    if (duration === 'forever') return 'Forever';
    if (duration === 'once') return 'Once';
    if (duration === 'repeating') return 'Repeating';
    return '';
  };

  return (
    <span className="d-inline-flex align-items-center gap-1">
      <Badge bg={isActive ? 'success' : 'secondary'} className="d-inline-flex align-items-center gap-1">
        {type === 'percent_off' ? <Percent size={12} /> : <Tag size={12} />}
        {getLabel()}
      </Badge>
      {duration && (
        <Badge bg="outline-secondary" className="border text-muted" style={{ fontSize: '0.7rem' }}>
          {getDurationLabel()}
        </Badge>
      )}
    </span>
  );
};
