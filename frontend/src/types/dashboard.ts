export interface DashboardStats {
  totalRevenue: number;
  netRevenue: number;
  mrrCurrent: number;
  activeSubscriptions: number;
  totalCustomers: number;
  revenueChange: number;
  subscriptionChange: number;
  customerChange: number;
}

export interface ActivityFeedItem {
  id?: string;
  type: string;
  title: string;
  description: string;
  status: string;
  amount: number | null;
  currency: string;
  timestamp: string;
  metadata?: any;
}
