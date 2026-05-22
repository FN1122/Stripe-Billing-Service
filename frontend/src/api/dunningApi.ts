import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { DunningConfig, DunningSchedule, DunningDashboard } from '../types/dunning';

export const dunningApi = {
  getConfig: () => apiClient.get<GatewayResponse<DunningConfig>>('/v1/dunning/config'),
  updateConfig: (data: any) => apiClient.put<GatewayResponse<DunningConfig>>('/v1/dunning/config', data),
  getSchedules: (filter?: any) => apiClient.get<PaginatedResponse<DunningSchedule>>('/v1/dunning/schedules', { params: filter }),
  getSchedule: (id: string) => apiClient.get<GatewayResponse<DunningSchedule>>(`/v1/dunning/schedules/${id}`),
  pauseSchedule: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/dunning/schedules/${id}/pause`),
  resumeSchedule: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/dunning/schedules/${id}/resume`),
  cancelSchedule: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/dunning/schedules/${id}/cancel`),
  manualRetry: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/dunning/schedules/${id}/retry`),
  getDashboard: () => apiClient.get<GatewayResponse<DunningDashboard>>('/v1/dunning/dashboard'),
};
