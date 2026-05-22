import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { UsageRecord, UsageSummary, MeterEvent, UsageDashboard } from '../types/usage';

export const usageApi = {
  reportUsage: (data: any) => apiClient.post<GatewayResponse<UsageRecord>>('/v1/usage/report', data),
  batchReportUsage: (data: any) => apiClient.post<GatewayResponse<UsageRecord[]>>('/v1/usage/report/batch', data),
  getUsageRecords: (filter?: any) => apiClient.get<PaginatedResponse<UsageRecord>>('/v1/usage', { params: filter }),
  getUsageSummary: (subscriptionId: string) => apiClient.get<GatewayResponse<UsageSummary>>(`/v1/usage/summary/${subscriptionId}`),
  createMeterEvent: (data: any) => apiClient.post<GatewayResponse<MeterEvent>>('/v1/usage/meter-events', data),
  getMeterEvents: (params?: any) => apiClient.get<PaginatedResponse<MeterEvent>>('/v1/usage/meter-events', { params }),
  getDashboard: () => apiClient.get<GatewayResponse<UsageDashboard>>('/v1/usage/dashboard'),
};
