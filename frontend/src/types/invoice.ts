export interface Invoice {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  stripeInvoiceId: string;
  invoiceNumber: string;
  subtotal: number;
  tax: number;
  total: number;
  amountPaid: number;
  amountDue: number;
  currency: string;
  status: string;
  invoicePdfUrl: string;
  hostedInvoiceUrl: string;
  paidAt: string | null;
  dueDate: string | null;
  createdAt: string;
}
