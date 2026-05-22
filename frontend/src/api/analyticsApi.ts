import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { MrrData, ChurnData, LtvData, SubscriptionMetrics } from '../types/analytics';

export const analyticsApi = {
  getMrrData: (startDate?: string, endDate?: string) =>
    apiClient.get<GatewayResponse<MrrData>>('/v1/analytics/mrr', {
      params: { startDate, endDate },
    }),

  getChurnData: (monthsBack: number = 12) =>
    apiClient.get<GatewayResponse<ChurnData>>('/v1/analytics/churn', {
      params: { monthsBack },
    }),

  getLtvData: () =>
    apiClient.get<GatewayResponse<LtvData>>('/v1/analytics/ltv'),

  getSubscriptionMetrics: (startDate?: string, endDate?: string) =>
    apiClient.get<GatewayResponse<SubscriptionMetrics>>('/v1/analytics/subscriptions', {
      params: { startDate, endDate },
    }),

  getRevenueByPlan: (startDate?: string, endDate?: string) =>
    apiClient.get<GatewayResponse<any>>('/v1/analytics/revenue-by-plan', {
      params: { startDate, endDate },
    }),

  getCustomerLifecycleMetrics: () =>
    apiClient.get<GatewayResponse<any>>('/v1/analytics/customer-lifecycle'),
};
