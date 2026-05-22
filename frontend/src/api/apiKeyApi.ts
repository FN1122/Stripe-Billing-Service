import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { ApiKey } from '../types/apiKey';

export const apiKeyApi = {
  getKeys: (page: number = 1, pageSize: number = 50) =>
    apiClient.get<PaginatedResponse<ApiKey>>('/v1/api-keys', {
      params: { page, pageSize },
    }),

  getKey: (id: string) =>
    apiClient.get<GatewayResponse<ApiKey>>(`/v1/api-keys/${id}`),

  createKey: (data: any) =>
    apiClient.post<GatewayResponse<ApiKey>>('/v1/api-keys', data),

  updateKey: (id: string, data: any) =>
    apiClient.put<GatewayResponse<ApiKey>>(`/v1/api-keys/${id}`, data),

  deleteKey: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/api-keys/${id}`),

  rotateKey: (id: string) =>
    apiClient.post<GatewayResponse<ApiKey>>(`/v1/api-keys/${id}/rotate`, {}),

  revokeKey: (id: string) =>
    apiClient.post<GatewayResponse<boolean>>(`/v1/api-keys/${id}/revoke`, {}),
};
