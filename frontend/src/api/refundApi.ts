import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Refund, RefundStats } from '../types/refund';

export const refundApi = {
  getRefunds: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<Refund>>('/v1/refunds', {
      params: { page, pageSize, ...filters },
    }),

  getRefund: (id: string) =>
    apiClient.get<GatewayResponse<Refund>>(`/v1/refunds/${id}`),

  createRefund: (data: any) =>
    apiClient.post<GatewayResponse<Refund>>('/v1/refunds', data),

  approveRefund: (id: string) =>
    apiClient.post<GatewayResponse<Refund>>(`/v1/refunds/${id}/approve`, {}),

  rejectRefund: (id: string, reason: string) =>
    apiClient.post<GatewayResponse<Refund>>(`/v1/refunds/${id}/reject`, { reason }),

  getStats: () =>
    apiClient.get<GatewayResponse<RefundStats>>('/v1/refunds/stats', {}),

  processRefund: (id: string) =>
    apiClient.post<GatewayResponse<Refund>>(`/v1/refunds/${id}/process`, {}),
};
