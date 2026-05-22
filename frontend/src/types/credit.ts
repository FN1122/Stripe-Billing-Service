export interface CreditTransaction {
  id: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  type: string;
  amount: number;
  currency: string;
  description: string;
  source: string;
  referenceId?: string;
  balanceAfter: number;
  createdAt: string;
}

export interface CustomerBalance {
  customerId: string;
  customerName?: string;
  currentBalance: number;
  currency: string;
  totalCredits: number;
  totalDebits: number;
  recentTransactions: CreditTransaction[];
}

export interface CreditsDashboard {
  totalOutstandingCredits: number;
  customersWithCredits: number;
  totalCreditsIssued: number;
  totalCreditsUsed: number;
}

export type CreditResponse = CreditTransaction;
