import React from 'react';
import { Badge } from 'react-bootstrap';
import { Receipt } from 'lucide-react';

interface TaxBadgeProps {
  behavior: string;
  rate?: number;
  exempt?: boolean;
}

export const TaxBadge: React.FC<TaxBadgeProps> = ({ behavior, rate, exempt = false }) => {
  if (exempt) {
    return <Badge bg="warning" className="d-inline-flex align-items-center gap-1"><Receipt size={12} /> Tax Exempt</Badge>;
  }

  return (
    <Badge bg={behavior === 'inclusive' ? 'info' : 'secondary'} className="d-inline-flex align-items-center gap-1">
      <Receipt size={12} />
      {rate !== undefined ? `${rate}%` : behavior}
    </Badge>
  );
};
