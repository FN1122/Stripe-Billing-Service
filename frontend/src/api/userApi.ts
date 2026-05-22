import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { User } from '../types/auth';

export const userApi = {
  getUsers: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<User>>('/v1/users', {
      params: { page, pageSize, ...filters },
    }),

  getUser: (id: string) =>
    apiClient.get<GatewayResponse<User>>(`/v1/users/${id}`),

  createUser: (data: any) =>
    apiClient.post<GatewayResponse<User>>('/v1/users', data),

  updateUser: (id: string, data: any) =>
    apiClient.put<GatewayResponse<User>>(`/v1/users/${id}`, data),

  deleteUser: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/users/${id}`),

  updateRole: (id: string, role: string) =>
    apiClient.post<GatewayResponse<User>>(`/v1/users/${id}/role`, { role }),

  toggleActive: (id: string) =>
    apiClient.post<GatewayResponse<User>>(`/v1/users/${id}/toggle-active`, {}),
};
