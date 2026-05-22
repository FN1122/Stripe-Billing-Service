import React from 'react';
import { StatusBadge } from './StatusBadge';

interface SubscriptionBadgeProps {
  status: string;
}

export const SubscriptionBadge: React.FC<SubscriptionBadgeProps> = ({ status }) => {
  let variant: 'success' | 'danger' | 'warning' | 'info' | 'default' = 'default';
  const lower = status.toLowerCase();

  if (lower === 'active') variant = 'success';
  else if (lower === 'cancelled') variant = 'danger';
  else if (lower === 'trialing' || lower === 'past_due') variant = 'warning';

  return <StatusBadge status={status} variant={variant} />;
};
