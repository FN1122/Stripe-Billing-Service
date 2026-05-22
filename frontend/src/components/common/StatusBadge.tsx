import React from 'react';
import './StatusBadge.scss';

interface StatusBadgeProps {
  status: string;
  variant?: 'success' | 'danger' | 'warning' | 'info' | 'default';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, variant }) => {
  const getVariant = (statusStr: string, variantProp?: string): string => {
    if (variantProp) return variantProp;
    if (!statusStr) return 'default';
    const lower = statusStr.toLowerCase();
    if (lower.includes('success') || lower.includes('completed') || lower.includes('active') || lower === 'paid')
      return 'success';
    if (lower.includes('failed') || lower.includes('error') || lower.includes('rejected') || lower === 'overdue')
      return 'danger';
    if (lower.includes('pending') || lower.includes('processing') || lower.includes('refunding'))
      return 'warning';
    if (lower.includes('trialing')) return 'info';
    return 'default';
  };

  return <span className={`status-badge variant-${getVariant(status, variant)}`}>{status || 'unknown'}</span>;
};
