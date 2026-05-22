import React from 'react';
import { Badge } from 'react-bootstrap';
import { AlertCircle, Mail, CreditCard, XCircle, CheckCircle } from 'lucide-react';

interface DunningStep {
  dayOffset: number;
  action: string;
  status?: string;
  executedAt?: string;
}

interface DunningTimelineProps {
  steps: DunningStep[];
  currentStep?: number;
}

export const DunningTimeline: React.FC<DunningTimelineProps> = ({ steps, currentStep = -1 }) => {
  const getIcon = (action: string) => {
    switch (action) {
      case 'send_reminder': return <Mail size={16} />;
      case 'retry_payment': return <CreditCard size={16} />;
      case 'cancel_subscription': return <XCircle size={16} />;
      default: return <AlertCircle size={16} />;
    }
  };

  const getStepColor = (idx: number, status?: string) => {
    if (status === 'completed') return '#22c55e';
    if (status === 'failed') return '#ef4444';
    if (idx === currentStep) return '#3b82f6';
    if (idx < currentStep) return '#22c55e';
    return '#d1d5db';
  };

  return (
    <div className="dunning-timeline">
      {steps.map((step, idx) => (
        <div key={idx} className="d-flex align-items-start mb-3">
          <div
            className="d-flex align-items-center justify-content-center rounded-circle flex-shrink-0"
            style={{
              width: 32, height: 32,
              backgroundColor: getStepColor(idx, step.status),
              color: 'white',
            }}
          >
            {step.status === 'completed' ? <CheckCircle size={16} /> : getIcon(step.action)}
          </div>
          <div className="ms-3">
            <div className="fw-medium" style={{ fontSize: '0.9rem' }}>
              Day {step.dayOffset}: {step.action.replace(/_/g, ' ')}
            </div>
            {step.executedAt && (
              <small className="text-muted">Executed: {new Date(step.executedAt).toLocaleDateString()}</small>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};
