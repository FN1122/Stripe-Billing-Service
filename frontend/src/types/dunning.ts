export interface DunningConfig {
  steps: DunningStepConfig[];
  gracePeriodDays: number;
  maxRetryAttempts: number;
  autoCancelAfterMaxRetries: boolean;
}

export interface DunningStepConfig {
  daysAfterFailure: number;
  action: string;
  emailTemplateKey?: string;
}

export interface DunningSchedule {
  id: string;
  subscriptionId: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  stripeInvoiceId?: string;
  status: string;
  currentStep: number;
  maxSteps: number;
  nextRetryAt?: string;
  lastRetryAt?: string;
  totalRetryAttempts: number;
  originalFailureDate: string;
  failureReason?: string;
  amountDue: number;
  currency: string;
  gracePeriodEndsAt?: string;
  createdAt: string;
}

export type DunningScheduleResponse = DunningSchedule;

export interface DunningDashboard {
  activeDunningCount: number;
  recoveredCount: number;
  lostCount: number;
  recoveryRate: number;
  totalAmountAtRisk: number;
  totalRecoveredAmount: number;
  byStep: Record<string, number>;
  recentActivity: DunningSchedule[];
}
