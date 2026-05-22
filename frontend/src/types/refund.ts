export interface Refund {
  id: string;
  transactionId: string;
  customerId: string | null;
  customerName: string;
  customerEmail: string;
  stripeRefundId: string;
  amount: number;
  currency: string;
  reason: string;
  notes: string;
  status: string;
  approvedBy: string;
  approvedAt: string | null;
  processedAt: string | null;
  createdAt: string;
}

export interface RefundStats {
  totalRefunds: number;
  totalAmount: number;
  refundRate: number;
  pendingCount: number;
  avgProcessingTimeHours: number;
}
