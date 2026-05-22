import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Invoice } from '../types/invoice';

export const invoiceApi = {
  getInvoices: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<Invoice>>('/v1/invoices', {
      params: { page, pageSize, ...filters },
    }),

  getInvoice: (id: string) =>
    apiClient.get<GatewayResponse<Invoice>>(`/v1/invoices/${id}`),

  getCustomerInvoices: (customerId: string, page: number = 1, pageSize: number = 50) =>
    apiClient.get<PaginatedResponse<Invoice>>(`/v1/customers/${customerId}/invoices`, {
      params: { page, pageSize },
    }),

  downloadPdf: (id: string) =>
    apiClient.get(`/v1/invoices/${id}/pdf`, { responseType: 'blob' }),

  sendInvoice: (id: string, email?: string) =>
    apiClient.post<GatewayResponse<boolean>>(`/v1/invoices/${id}/send`, { email }),

  createInvoice: (data: any) =>
    apiClient.post<GatewayResponse<Invoice>>('/v1/invoices', data),

  markAsPaid: (id: string) =>
    apiClient.post<GatewayResponse<Invoice>>(`/v1/invoices/${id}/mark-paid`, {}),
};
