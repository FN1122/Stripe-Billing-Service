import React from 'react';
import { Modal, Button } from 'react-bootstrap';
import { Download, Eye } from 'lucide-react';
import { Invoice } from '../../types/invoice';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { StatusBadge } from './StatusBadge';

interface InvoiceViewerProps {
  isOpen: boolean;
  invoice: Invoice | null;
  onClose: () => void;
  onDownload?: () => void;
}

export const InvoiceViewer: React.FC<InvoiceViewerProps> = ({
  isOpen,
  invoice,
  onClose,
  onDownload,
}) => {
  if (!invoice) return null;

  return (
    <Modal show={isOpen} onHide={onClose} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>Invoice {invoice.invoiceNumber}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <div className="invoice-details">
          <div className="invoice-row">
            <span className="label">Customer:</span>
            <span className="value">{invoice.customerName}</span>
          </div>
          <div className="invoice-row">
            <span className="label">Email:</span>
            <span className="value">{invoice.customerEmail}</span>
          </div>
          <div className="invoice-row">
            <span className="label">Status:</span>
            <StatusBadge status={invoice.status} />
          </div>
          <div className="invoice-row">
            <span className="label">Created:</span>
            <span className="value">{formatDate(invoice.createdAt)}</span>
          </div>
          <div className="invoice-row">
            <span className="label">Subtotal:</span>
            <span className="value">{formatCurrency(invoice.subtotal, invoice.currency)}</span>
          </div>
          <div className="invoice-row">
            <span className="label">Tax:</span>
            <span className="value">{formatCurrency(invoice.tax, invoice.currency)}</span>
          </div>
          <div className="invoice-row total">
            <span className="label">Total:</span>
            <span className="value">{formatCurrency(invoice.total, invoice.currency)}</span>
          </div>
        </div>
      </Modal.Body>
      <Modal.Footer>
        {onDownload && (
          <Button variant="outline-primary" onClick={onDownload}>
            <Download size={16} /> Download
          </Button>
        )}
        {invoice.hostedInvoiceUrl && (
          <Button
            variant="outline-primary"
            href={invoice.hostedInvoiceUrl}
            target="_blank"
          >
            <Eye size={16} /> View
          </Button>
        )}
        <Button variant="secondary" onClick={onClose}>
          Close
        </Button>
      </Modal.Footer>
    </Modal>
  );
};
