import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';

export const invoiceItemApi = {
  create: (data: any) => apiClient.post<GatewayResponse<any>>('/v1/invoice-items', data),
  list: (params?: any) => apiClient.get<GatewayResponse<any[]>>('/v1/invoice-items', { params }),
  remove: (id: string) => apiClient.delete<GatewayResponse<boolean>>(`/v1/invoice-items/${id}`),
  getUpcomingInvoice: (subscriptionId: string) => apiClient.get<GatewayResponse<any>>(`/v1/invoice-items/upcoming/${subscriptionId}`),
};
