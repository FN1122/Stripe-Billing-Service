import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { DashboardStats, ActivityFeedItem } from '../types/dashboard';

export const dashboardApi = {
  getStats: () =>
    apiClient.get<GatewayResponse<DashboardStats>>('/v1/dashboard/stats'),

  getActivityFeed: (limit: number = 10) =>
    apiClient.get<GatewayResponse<ActivityFeedItem[]>>('/v1/dashboard/activity', {
      params: { count: limit },
    }),

  getRecentTransactions: (limit: number = 5) =>
    apiClient.get<GatewayResponse<any[]>>('/v1/dashboard/recent-transactions', {
      params: { limit },
    }),

  getRevenueChart: (days: number = 30) =>
    apiClient.get<GatewayResponse<any>>('/v1/dashboard/revenue-chart', {
      params: { days },
    }),
};
