import React, { useState } from 'react';
import { Button, Spinner } from 'react-bootstrap';
import { Download } from 'lucide-react';

interface ExportButtonProps {
  onExport: () => Promise<Blob>;
  filename: string;
  label?: string;
  variant?: string;
  size?: 'sm' | 'lg';
}

export const ExportButton: React.FC<ExportButtonProps> = ({ onExport, filename, label = 'Export', variant = 'outline-primary', size = 'sm' }) => {
  const [isExporting, setIsExporting] = useState(false);

  const handleExport = async () => {
    try {
      setIsExporting(true);
      const blob = await onExport();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Export failed:', err);
    } finally {
      setIsExporting(false);
    }
  };

  return (
    <Button variant={variant} size={size} onClick={handleExport} disabled={isExporting}>
      {isExporting ? <Spinner animation="border" size="sm" /> : <Download size={16} className="me-1" />}
      {label}
    </Button>
  );
};
