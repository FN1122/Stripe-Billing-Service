export interface UsageRecord {
  id: string;
  subscriptionId: string;
  stripeSubscriptionItemId?: string;
  quantity: number;
  timestamp: string;
  action: string;
  idempotencyKey?: string;
  createdAt: string;
}

// Alias used by UsageBillingPage
export type UsageRecordResponse = UsageRecord;

export interface UsageSummary {
  subscriptionId: string;
  customerName?: string;
  currentPeriodUsage: number;
  previousPeriodUsage: number;
  usageChangePercent: number;
  estimatedCharge: number;
  dailyUsage: Record<string, number>;
}

export interface MeterEvent {
  id: string;
  customerId: string;
  eventName: string;
  value: number;
  timestamp: string;
  properties?: string;
  createdAt: string;
}

export interface UsageDashboard {
  totalUsageCurrentPeriod: number;
  activeMeteredSubscriptions: number;
  estimatedRevenue: number;
  topConsumers: TopConsumer[];
  usageTrend: Record<string, number>;
}

export interface TopConsumer {
  customerId: string;
  customerName?: string;
  totalUsage: number;
  estimatedCharge: number;
}
