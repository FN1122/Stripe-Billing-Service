import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { AuditLogEntry } from '../types/audit';

export const auditApi = {
  getLogs: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<AuditLogEntry>>('/v1/audit-logs', {
      params: { page, pageSize, ...filters },
    }),

  getLog: (id: string) =>
    apiClient.get<GatewayResponse<AuditLogEntry>>(`/v1/audit-logs/${id}`),

  getByUser: (userId: string, page: number = 1, pageSize: number = 50) =>
    apiClient.get<PaginatedResponse<AuditLogEntry>>('/v1/audit-logs/user', {
      params: { userId, page, pageSize },
    }),

  getByEntity: (entityType: string, entityId: string) =>
    apiClient.get<GatewayResponse<AuditLogEntry[]>>('/v1/audit-logs/entity', {
      params: { entityType, entityId },
    }),
};
