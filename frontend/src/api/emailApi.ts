import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { EmailTemplate, EmailLog, EmailStats } from '../types/email';

export const emailApi = {
  getTemplates: () => apiClient.get<GatewayResponse<EmailTemplate[]>>('/v1/emails/templates'),
  getTemplate: (key: string) => apiClient.get<GatewayResponse<EmailTemplate>>(`/v1/emails/templates/${key}`),
  createTemplate: (data: any) => apiClient.post<GatewayResponse<EmailTemplate>>('/v1/emails/templates', data),
  updateTemplate: (key: string, data: any) => apiClient.put<GatewayResponse<EmailTemplate>>(`/v1/emails/templates/${key}`, data),
  resetTemplate: (key: string) => apiClient.post<GatewayResponse<EmailTemplate>>(`/v1/emails/templates/${key}/reset`),
  previewTemplate: (keyOrParams: string | { templateId: string; variables: Record<string, string> }, variables?: Record<string, string>) => {
    if (typeof keyOrParams === 'object') {
      return apiClient.post<GatewayResponse<string>>(`/v1/emails/templates/${keyOrParams.templateId}/preview`, { variables: keyOrParams.variables });
    }
    return apiClient.post<GatewayResponse<string>>(`/v1/emails/templates/${keyOrParams}/preview`, { variables: variables || {} });
  },
  getLogs: (filter?: any) => apiClient.get<PaginatedResponse<EmailLog>>('/v1/emails/logs', { params: filter }),
  getEmailLogs: (filter?: any) => apiClient.get<PaginatedResponse<EmailLog>>('/v1/emails/logs', { params: filter }),
  resendEmail: (id: string) => apiClient.post<GatewayResponse<EmailLog>>(`/v1/emails/logs/${id}/resend`),
  sendEmail: (data: any) => apiClient.post<GatewayResponse<EmailLog>>('/v1/emails/send', data),
  getStats: () => apiClient.get<GatewayResponse<EmailStats>>('/v1/emails/stats'),
};
