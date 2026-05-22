import React from 'react';
import { StatusBadge } from './StatusBadge';

interface RefundBadgeProps {
  status: string;
}

export const RefundBadge: React.FC<RefundBadgeProps> = ({ status }) => {
  let variant: 'success' | 'danger' | 'warning' | 'info' | 'default' = 'default';
  const lower = status.toLowerCase();

  if (lower === 'completed') variant = 'success';
  else if (lower === 'failed' || lower === 'rejected') variant = 'danger';
  else if (lower === 'pending' || lower === 'processing') variant = 'warning';

  return <StatusBadge status={status} variant={variant} />;
};
