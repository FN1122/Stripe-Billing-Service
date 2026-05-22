import React, { useState } from 'react';
import { InputGroup, Form, Button } from 'react-bootstrap';
import { Copy, Check } from 'lucide-react';

interface CodeCopyFieldProps {
  value: string;
  label?: string;
}

export const CodeCopyField: React.FC<CodeCopyFieldProps> = ({ value, label }) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(value);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div>
      {label && <Form.Label className="text-muted" style={{ fontSize: '0.8rem' }}>{label}</Form.Label>}
      <InputGroup size="sm">
        <Form.Control readOnly value={value} style={{ fontFamily: 'monospace', fontSize: '0.85rem', background: '#f8f9fa' }} />
        <Button variant="outline-secondary" onClick={handleCopy}>
          {copied ? <Check size={14} className="text-success" /> : <Copy size={14} />}
        </Button>
      </InputGroup>
    </div>
  );
};
