import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { RateLimit, RateLimitUsage } from '../types/rateLimit';

export const rateLimitApi = {
  list: () => apiClient.get<GatewayResponse<RateLimit[]>>('/v1/rate-limits'),
  create: (data: any) => apiClient.post<GatewayResponse<RateLimit>>('/v1/rate-limits', data),
  update: (id: string, data: any) => apiClient.put<GatewayResponse<RateLimit>>(`/v1/rate-limits/${id}`, data),
  remove: (id: string) => apiClient.delete<GatewayResponse<boolean>>(`/v1/rate-limits/${id}`),
  getUsage: () => apiClient.get<GatewayResponse<RateLimitUsage[]>>('/v1/rate-limits/usage'),
};
