import React from 'react';
import { InputGroup, Form } from 'react-bootstrap';

interface AmountInputProps {
  value: number;
  onChange: (value: number) => void;
  currency?: string;
  label?: string;
  placeholder?: string;
}

export const AmountInput: React.FC<AmountInputProps> = ({ value, onChange, currency = 'USD', label, placeholder = '0.00' }) => {
  return (
    <div>
      {label && <Form.Label>{label}</Form.Label>}
      <InputGroup>
        <InputGroup.Text>{currency.toUpperCase()}</InputGroup.Text>
        <Form.Control
          type="number"
          step="0.01"
          min="0"
          value={(value / 100).toFixed(2)}
          placeholder={placeholder}
          onChange={(e) => onChange(Math.round(parseFloat(e.target.value || '0') * 100))}
        />
      </InputGroup>
    </div>
  );
};
