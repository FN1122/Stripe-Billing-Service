import React from 'react';
import { TrendingUp, TrendingDown } from 'lucide-react';
import './MetricCard.scss';

interface MetricCardProps {
  label: string;
  value: string | number;
  change?: number;
  icon?: React.ReactNode;
  trend?: 'up' | 'down' | 'neutral';
}

export const MetricCard: React.FC<MetricCardProps> = ({
  label,
  value,
  change,
  icon,
  trend = 'neutral',
}) => {
  return (
    <div className="metric-card">
      <div className="metric-header">
        <h3 className="metric-label">{label}</h3>
        {icon && <div className="metric-icon">{icon}</div>}
      </div>
      <div className="metric-value">{value}</div>
      {change !== undefined && (
        <div className={`metric-change ${trend}`}>
          {trend === 'up' && <TrendingUp size={16} />}
          {trend === 'down' && <TrendingDown size={16} />}
          <span>{Math.abs(change)}%</span>
        </div>
      )}
    </div>
  );
};
