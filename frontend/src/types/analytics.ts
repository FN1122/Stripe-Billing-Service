export interface MrrData {
  currentMrr: number;
  previousMrr: number;
  mrrGrowth: number;
  newMrr: number;
  expansionMrr: number;
  contractionMrr: number;
  churnedMrr: number;
  netNewMrr: number;
  mrrHistory: { month: string; mrr: number }[];
}

export interface ChurnData {
  monthlyChurnRate: number;
  annualChurnRate: number;
  churnedSubscriptions: number;
  churnedMrr: number;
  retentionRate: number;
}

export interface LtvData {
  averageLtv: number;
  medianLtv: number;
  averageSubscriptionDurationMonths: number;
  averageRevenuePerCustomer: number;
}

export interface SubscriptionMetrics {
  activeCount: number;
  trialingCount: number;
  pastDueCount: number;
  cancelledThisMonth: number;
  newThisMonth: number;
  trend: { date: string; newCount: number; cancelledCount: number }[];
}
