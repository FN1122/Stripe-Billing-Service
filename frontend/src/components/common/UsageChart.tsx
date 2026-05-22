import React from 'react';
import { Card } from 'react-bootstrap';

interface UsageDataPoint {
  date: string;
  quantity: number;
}

interface UsageChartProps {
  data: UsageDataPoint[];
  title?: string;
  color?: string;
}

export const UsageChart: React.FC<UsageChartProps> = ({ data, title = 'Usage Over Time', color = '#4f46e5' }) => {
  const maxValue = Math.max(...data.map(d => d.quantity), 1);

  return (
    <Card className="border-0 shadow-sm">
      <Card.Body>
        <h6 className="text-muted mb-3">{title}</h6>
        <div className="d-flex align-items-end gap-1" style={{ height: '120px' }}>
          {data.map((point, idx) => (
            <div
              key={idx}
              className="flex-fill"
              style={{
                background: color,
                height: `${(point.quantity / maxValue) * 100}%`,
                minHeight: '2px',
                borderRadius: '2px 2px 0 0',
                opacity: 0.8,
              }}
              title={`${point.date}: ${point.quantity}`}
            />
          ))}
        </div>
        {data.length > 0 && (
          <div className="d-flex justify-content-between mt-1">
            <small className="text-muted">{data[0]?.date}</small>
            <small className="text-muted">{data[data.length - 1]?.date}</small>
          </div>
        )}
      </Card.Body>
    </Card>
  );
};
