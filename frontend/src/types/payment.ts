export interface PaymentTransaction {
  id: string;
  customerId: string | null;
  customerName: string;
  customerEmail: string;
  stripePaymentIntentId: string;
  amount: number;
  amountRefunded: number;
  currency: string;
  status: string;
  type: string;
  paymentMethod: string;
  paymentMethodLast4: string;
  paymentMethodBrand: string;
  description: string;
  failureReason: string;
  receiptUrl: string;
  createdAt: string;
}

export interface PaymentAnalytics {
  totalRevenue: number;
  netRevenue: number;
  transactionCount: number;
  successCount: number;
  failedCount: number;
  successRate: number;
  averageTransactionValue: number;
  revenueByDay: { date: string; amount: number; count: number }[];
}

export interface CreateCheckoutRequest {
  customerId?: string;
  customerEmail?: string;
  lineItems: { name: string; amount: number; currency: string; quantity: number }[];
  successUrl: string;
  cancelUrl: string;
  mode: 'payment' | 'subscription';
}
