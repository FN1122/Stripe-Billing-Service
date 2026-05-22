import React from 'react';
import { StatusBadge } from './StatusBadge';

interface WebhookStatusBadgeProps {
  status: string;
}

export const WebhookStatusBadge: React.FC<WebhookStatusBadgeProps> = ({ status }) => {
  let variant: 'success' | 'danger' | 'warning' | 'info' | 'default' = 'default';
  const lower = status.toLowerCase();

  if (lower === 'delivered' || lower === 'success') variant = 'success';
  else if (lower === 'failed') variant = 'danger';
  else if (lower === 'pending' || lower === 'retrying') variant = 'warning';

  return <StatusBadge status={status} variant={variant} />;
};
