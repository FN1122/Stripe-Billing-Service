import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { LogEntry, LogStats } from '../types/log';

export const logApi = {
  getLogs: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<LogEntry>>('/v1/logs', {
      params: { page, pageSize, ...filters },
    }),

  getLog: (id: string) =>
    apiClient.get<GatewayResponse<LogEntry>>(`/v1/logs/${id}`),

  getStats: (startDate?: string, endDate?: string) =>
    apiClient.get<GatewayResponse<LogStats>>('/v1/logs/stats', {
      params: { startDate, endDate },
    }),

  clearLogs: (olderThanDays: number = 30) =>
    apiClient.post<GatewayResponse<boolean>>('/v1/logs/clear', { olderThanDays }),
};
