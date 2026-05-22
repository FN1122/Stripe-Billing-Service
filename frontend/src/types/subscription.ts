export interface Subscription {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  planId: string;
  planName: string;
  planAmount: number;
  stripeSubscriptionId: string;
  status: string;
  quantity: number;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  trialEnd: string | null;
  cancelAtPeriodEnd: boolean;
  cancelledAt: string | null;
  cancellationReason: string;
  createdAt: string;
}

export interface ProrationPreview {
  currentPlan: SubscriptionPlan;
  newPlan: SubscriptionPlan;
  proratedAmount: number;
  effectiveDate: string;
  immediateCharge: number;
  nextInvoiceAmount: number;
}

export interface SubscriptionPlan {
  id: string;
  stripeProductId: string;
  stripePriceId: string;
  name: string;
  description: string;
  amount: number;
  currency: string;
  interval: string;
  intervalCount: number;
  trialDays: number;
  features: string[];
  sortOrder: number;
  isActive: boolean;
  subscriberCount: number;
  createdAt: string;
}
