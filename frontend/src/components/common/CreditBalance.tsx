import React from 'react';
import { Card, Badge } from 'react-bootstrap';
import { Wallet, TrendingUp, TrendingDown } from 'lucide-react';

interface CreditBalanceProps {
  balance: number;
  currency?: string;
  totalCredits?: number;
  totalDebits?: number;
}

export const CreditBalance: React.FC<CreditBalanceProps> = ({ balance, currency = 'USD', totalCredits, totalDebits }) => {
  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount / 100);
  };

  return (
    <Card className="border-0 shadow-sm">
      <Card.Body className="text-center">
        <Wallet size={24} className="text-primary mb-2" />
        <h6 className="text-muted mb-1">Credit Balance</h6>
        <h3 className="mb-3" style={{ color: balance >= 0 ? '#22c55e' : '#ef4444' }}>
          {formatAmount(balance)}
        </h3>
        {(totalCredits !== undefined || totalDebits !== undefined) && (
          <div className="d-flex justify-content-around">
            {totalCredits !== undefined && (
              <div>
                <Badge bg="success" className="d-inline-flex align-items-center gap-1">
                  <TrendingUp size={12} /> {formatAmount(totalCredits)}
                </Badge>
                <div className="text-muted" style={{ fontSize: '0.7rem' }}>Credits</div>
              </div>
            )}
            {totalDebits !== undefined && (
              <div>
                <Badge bg="danger" className="d-inline-flex align-items-center gap-1">
                  <TrendingDown size={12} /> {formatAmount(totalDebits)}
                </Badge>
                <div className="text-muted" style={{ fontSize: '0.7rem' }}>Debits</div>
              </div>
            )}
          </div>
        )}
      </Card.Body>
    </Card>
  );
};
