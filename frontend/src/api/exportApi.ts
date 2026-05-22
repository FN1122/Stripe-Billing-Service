import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';

export const exportApi = {
  exportTransactions: (params?: any) => apiClient.get('/v1/exports/transactions', { params, responseType: 'blob' }),
  exportInvoices: (params?: any) => apiClient.get('/v1/exports/invoices', { params, responseType: 'blob' }),
  exportCustomers: (params?: any) => apiClient.get('/v1/exports/customers', { params, responseType: 'blob' }),
  exportSubscriptions: (params?: any) => apiClient.get('/v1/exports/subscriptions', { params, responseType: 'blob' }),
  exportRefunds: (params?: any) => apiClient.get('/v1/exports/refunds', { params, responseType: 'blob' }),
  exportAuditLog: (params?: any) => apiClient.get('/v1/exports/audit-log', { params, responseType: 'blob' }),
  generateRevenueReport: (from: string, to: string) => apiClient.get('/v1/exports/reports/revenue', { params: { from, to }, responseType: 'blob' }),
  generateTaxReport: (from: string, to: string) => apiClient.get('/v1/exports/reports/tax', { params: { from, to }, responseType: 'blob' }),
  getExportHistory: () => apiClient.get<GatewayResponse<any[]>>('/v1/exports/history'),
  scheduleReport: (data: any) => apiClient.post<GatewayResponse<boolean>>('/v1/exports/schedule', data),
};
